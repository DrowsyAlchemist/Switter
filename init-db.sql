-- Создаем расширения
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
CREATE EXTENSION IF NOT EXISTS "pg_stat_statements";

-- Создаем схему для логирования (опционально)
CREATE SCHEMA IF NOT EXISTS logs;