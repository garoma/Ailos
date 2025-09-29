using Dapper;
using Microsoft.Data.Sqlite;

namespace Questao5.Infrastructure.Sqlite
{
    public class DatabaseBootstrap : IDatabaseBootstrap
    {
        private readonly DatabaseConfig databaseConfig;

        public DatabaseBootstrap(DatabaseConfig databaseConfig)
        {
            this.databaseConfig = databaseConfig;
        }

        public void Setup()
        {
            using var connection = new SqliteConnection(databaseConfig.Name);

            var table = connection.Query<string>("SELECT name FROM sqlite_master WHERE type='table' AND (name = 'contacorrente' or name = 'movimento' or name = 'idempotencia');");
            var tableName = table.FirstOrDefault();
            if (!string.IsNullOrEmpty(tableName) && (tableName == "contacorrente" || tableName == "movimento" || tableName == "idempotencia"))
                return;

            connection.Execute(
                "CREATE TABLE contacorrente ( " +
                "idcontacorrente TEXT(37) PRIMARY KEY, " +
                "numero TEXT(10) NOT NULL UNIQUE, " +
                "nome TEXT(100) NOT NULL, " +
                "cpf TEXT(14) NOT NULL UNIQUE, " +
                "senha TEXT(256) NOT NULL, " +
                "salt TEXT(100) NOT NULL, " +
                "ativo INTEGER(1) NOT NULL DEFAULT 1, " +
                "CHECK (ativo IN (0, 1)) " +
                ");");

            connection.Execute(
                "CREATE TABLE movimento (" +
                "idmovimento TEXT(37) PRIMARY KEY, " +
                "idcontacorrente TEXT(37) NOT NULL, " +
                "datahora TEXT NOT NULL, " +
                "tipomovimento TEXT(1) NOT NULL, " +
                "valor REAL NOT NULL CHECK(valor >= 0), " +
                "CHECK(tipomovimento IN ('C', 'D'))," +
                "FOREIGN KEY(idcontacorrente) REFERENCES contacorrente(idcontacorrente)" +
                ");"
            );

            connection.Execute(
                "CREATE TABLE transferencia (" +
                "idtransferencia TEXT(37) PRIMARY KEY, " +
                "origem_id TEXT(37) NOT NULL, " +
                "destino_id TEXT(37) NOT NULL, " +
                "datahora TEXT NOT NULL, " +
                "valor REAL NOT NULL CHECK(valor > 0), " +
                "FOREIGN KEY(origem_id) REFERENCES contacorrente(idcontacorrente), " +
                "FOREIGN KEY(destino_id) REFERENCES contacorrente(idcontacorrente)" +
                ");"
            );

            connection.Execute(
                "CREATE TABLE tarifa (" +
                "idtarifa TEXT(37) PRIMARY KEY, " +
                "idcontacorrente TEXT(37) NOT NULL, " +
                "descricao TEXT NOT NULL, " +
                "valor REAL NOT NULL CHECK(valor >= 0), " +
                "datahora TEXT NOT NULL, " +
                "FOREIGN KEY(idcontacorrente) REFERENCES contacorrente(idcontacorrente)" +
                ");"
            );

            connection.Execute(
                "CREATE TABLE idempotencia (" +
                "chave_idempotencia TEXT(100) PRIMARY KEY, " +
                "requisicao TEXT NOT NULL, " +
                "resultado TEXT NOT NULL, " +
                "datahora TEXT NOT NULL" +
                ");"
            );
        }
    }
}
