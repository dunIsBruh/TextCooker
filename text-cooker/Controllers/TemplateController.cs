namespace text_cooker.Controllers;

using System.Text.Json;
using Core;
using Core.Attributes;
using Core.Interfaces;
using Entities;
using Helper;
using Models;


public class TemplateController(ITemplateRepository templateRepository)
{
    /// <summary>
    /// GET /template/all
    /// получение всех шаблонов пользователя (его + публичных)
    /// </summary>
    public async Task GetAll(ContextEx ctx)
    {
        int userId = ctx.CurrentUser.Id;

        var templates = await templateRepository.GetAvailableAsync(userId);

        await ctx.Ok(templates);
    }

    /// <summary>
    /// POST /templates/create
    /// создание нового шаблона
    /// </summary>
    public async Task CreateTemplate([Body] CreateTemplateRequest body, ContextEx ctx)
    {
        int userId = ctx.CurrentUser.Id;

        var template = new Template
        {
            Name = body.Name,
            Text = body.RawText,
            OwnerId = userId,
            CreatedAt = DateTime.UtcNow,
            IsPublic = body.IsPublic
        };

        int id = await templateRepository.CreateAsync(template);

        await ctx.Created(new { template_id = id });
    }
}
