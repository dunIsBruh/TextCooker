using System.Text.Json;
using text_cooker.Core;
using text_cooker.Core.Attributes;
using text_cooker.Core.Interfaces;
using text_cooker.Entities;
using text_cooker.Helper;
using text_cooker.Models;
using text_cooker.Pipeline;

namespace text_cooker.Controllers;

public class DocumentController(
    IDocumentRepository docs,
    ITemplateRepository templates,
    IDocumentCommitRepository commits,
    ITemplateEngine templateEngine)
{

    // CREATE DOCUMENT
    public async Task CreateDocument([Body] CreateDocumentRequest body, ContextEx ctx)
    {
        int userId = ctx.CurrentUser.Id;

        var documentId = await docs.CreateAsync(new Document
        {
            Title = body.Title,
            OwnerId = userId
        });

        await commits.CreateAsync(new DocumentCommit
        {
            DocumentId = documentId,
            VersionNumber = 1,
            Snapshot = body.InitialText
        });

        await ctx.Created(new { document_id = documentId });
    }

    public async Task GetAll(ContextEx ctx)
    {
        int userId = ctx.CurrentUser.Id;
        var documents = await docs.GetAllAsync(userId);
        
        var documentsWithPreview = new List<object>();
        foreach (var doc in documents)
        {
            var lastCommit = await commits.GetLastAsync(doc.DocumentId);
            var previewText = lastCommit?.Snapshot ?? "";
            var preview = previewText.Length > 40 ? previewText.Substring(0, 40) : previewText;
            
            documentsWithPreview.Add(new
            {
                documentId = doc.DocumentId,
                title = doc.Title,
                ownerId = doc.OwnerId,
                createdAt = doc.CreatedAt,
                updatedAt = doc.UpdatedAt,
                previewText = preview
            });
        }
        
        await ctx.Ok(documentsWithPreview);
    }

    
    // FULL COMMIT (apply template to entire text)
    public async Task FullCommit([Route("id")] int documentId, [Body] ApplyTemplateRequest body, ContextEx ctx)
    {
        int userId = ctx.CurrentUser.Id;

        var document = await docs.GetAsync(documentId);
        if (document == null || document.OwnerId != userId)
            throw new ForbiddenException();

        var last = await commits.GetLastAsync(documentId);
        if (last == null)
        {
            await ctx.NotFound($"Document with id {documentId} not found");
            return;
        }
            
        var lastSnapshot = last.Snapshot;

        var template = await templates.GetAsync(body.TemplateId);
        if (template is null)
        {
            await ctx.NotFound(new { message = $"template {body.TemplateId} not found" });
            return;
        }

        string newText = await templateEngine.RewriteByTemplate(template.Text, lastSnapshot);

        await commits.CreateAsync(new DocumentCommit
        {
            DocumentId = documentId,
            VersionNumber = last.VersionNumber + 1,
            TemplateId = body.TemplateId,
            Snapshot = newText,
            ChangeStart = 0,
            ChangeEnd = lastSnapshot.Length
        });

        await docs.UpdateTimestampAsync(documentId);

        await ctx.Ok(new { message = "template applied to full text" });
    }


    // PARTIAL COMMIT
    public async Task PartialCommit([Route("id")] int documentId, [Body] PartialUpdateRequest body, ContextEx ctx)
    {
        int userId = ctx.CurrentUser.Id;

        var document = await docs.GetAsync(documentId);
        if (document == null || document.OwnerId != userId)
            throw new ForbiddenException();

        var last = await commits.GetLastAsync(documentId);
        if (last == null)
        {
            await ctx.NotFound($"Document with id {documentId} not found");
            return;
        }
        
        var snapshot = last.Snapshot;

        string original = snapshot;

        if (body.Start < 0 || body.End > original.Length || body.Start >= body.End)
        {
            await ctx.BadRequest(new { error = "invalid start/end" });
            return;
        }

        var template = await templates.GetAsync(body.TemplateId);
        if (template is null)
        {
            await ctx.NotFound(new { message = $"template {body.TemplateId} not found" });
            return;
        }

        string fragment = original.Substring(body.Start, body.End - body.Start);

        string rewritten = await templateEngine.RewriteByTemplate(
            template.Text,
            fragment
        );

        string merged =
            original[..body.Start] +
            rewritten +
            original[body.End..];

        await commits.CreateAsync(new DocumentCommit
        {
            DocumentId = documentId,
            VersionNumber = last.VersionNumber + 1,
            TemplateId = body.TemplateId,
            ChangeStart = body.Start,
            ChangeEnd = body.End,
            Snapshot = merged
        });

        await docs.UpdateTimestampAsync(documentId);

        await ctx.Ok(new { message = "text fragment updated" });
    }


    // HISTORY
    public async Task GetHistory([Route("id")] int documentId, ContextEx ctx)
    {
        int userId = ctx.CurrentUser.Id;

        var document = await docs.GetAsync(documentId);
        if (document == null || document.OwnerId != userId)
            throw new ForbiddenException();

        var history = await commits.GetHistoryAsync(documentId);

        await ctx.Ok(history);
    }
    
    
    // ROLLBACK
    public async Task Rollback(
        [Route("id")] int documentId,
        [Route("version")] int version,
        ContextEx ctx)
    {
        int userId = ctx.CurrentUser.Id;

        var document = await docs.GetAsync(documentId);
        if (document == null || document.OwnerId != userId)
            throw new ForbiddenException();

        var history = await commits.GetHistoryAsync(documentId);
        var target = history.FirstOrDefault(h => h.VersionNumber == version);

        if (target is null)
        {
            await ctx.NotFound(new { message = "version not found" });
            return;
        }

        var last = history.Last();

        await commits.CreateAsync(new DocumentCommit
        {
            DocumentId = documentId,
            VersionNumber = last.VersionNumber + 1,
            Snapshot = target.Snapshot,
            ChangeStart = 0,
            ChangeEnd = target.Snapshot.Length,
            TemplateId = target.TemplateId
        });

        await docs.UpdateTimestampAsync(documentId);

        await ctx.Ok(new { message = $"rolled back to version {version}" });
    }
}
