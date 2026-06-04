-- Создание схемы
CREATE SCHEMA IF NOT EXISTS text_cooker;
SET search_path TO text_cooker;

-- ==============================
-- Таблица: Users
-- ==============================
CREATE TABLE Users (
    user_id SERIAL PRIMARY KEY,
    login VARCHAR(100) NOT NULL,
    password_hash VARCHAR(255) NOT NULL,
    role VARCHAR(50) NOT NULL DEFAULT 'user',
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    last_login TIMESTAMP,
    
    CONSTRAINT uq_login UNIQUE (login)
);

-- ==============================
-- Таблица: RefreshToken
-- ==============================

CREATE TABLE RefreshToken (
    refresh_token_id SERIAL PRIMARY KEY,
    user_id INT,
    token_hash TEXT NOT NULL,
    expires_at TIMESTAMP NOT NULL,
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    create_by_ip TEXT,
    revoked_at TIMESTAMP,
    replaced_by_token_hash TEXT,
    
    CONSTRAINT fk_template_user
      FOREIGN KEY (user_id) REFERENCES Users(user_id)
          ON DELETE CASCADE,
    
    CONSTRAINT uq_refresh_token_hash UNIQUE (token_hash)
);

-- ==============================
-- Таблица: Templates
-- ==============================
CREATE TABLE Templates (
    template_id SERIAL PRIMARY KEY,
    name VARCHAR(255) NOT NULL,
    structure JSONB NOT NULL,
    owner_id INT NOT NULL,
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    is_public BOOLEAN NOT NULL DEFAULT FALSE,
    CONSTRAINT fk_template_owner
       FOREIGN KEY (owner_id) REFERENCES Users(user_id)
           ON DELETE CASCADE
);

-- ==============================
-- Таблица: Documents
-- ==============================
CREATE TABLE Documents (
    document_id SERIAL PRIMARY KEY,
    title VARCHAR(255) NOT NULL,
    owner_id INT NOT NULL,
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP NOT NULL DEFAULT NOW(),
    CONSTRAINT fk_document_owner
       FOREIGN KEY (owner_id) REFERENCES Users(user_id)
           ON DELETE CASCADE
);

-- ==============================
-- Таблица: DocumentCommit
-- ==============================
CREATE TABLE DocumentCommit (
    commit_id SERIAL PRIMARY KEY,
    document_id INT NOT NULL,
    changed_section TEXT,
    version_number INT NOT NULL,
    template_id INT,
    snapshot JSONB,
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    CONSTRAINT fk_commit_document
        FOREIGN KEY (document_id) REFERENCES Documents(document_id)
            ON DELETE CASCADE,
    CONSTRAINT fk_document_template
        FOREIGN KEY (template_id) REFERENCES Templates(template_id)
            ON DELETE SET NULL
);
