
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;

namespace CourseWork_Zubarev
{
    public static class DatabaseService
    {
        private static readonly string DbPath =
            Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "plagiarism.db"
            );

        private static string ConnectionString
        {
            get
            {
                return "Data Source=" + DbPath +
                       ";Version=3;";
            }
        }

        public static void Initialize()
        {
            string directory = Path.GetDirectoryName(DbPath);

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            if (!File.Exists(DbPath))
            {
                SQLiteConnection.CreateFile(DbPath);
            }

            using (SQLiteConnection connection =
                   new SQLiteConnection(ConnectionString))
            {
                connection.Open();

                string sql =
                    @"CREATE TABLE IF NOT EXISTS Documents
                    (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Title TEXT NOT NULL,
                        Content TEXT NOT NULL,
                        CreatedAt TEXT NOT NULL
                    );";

                using (SQLiteCommand command =
                       new SQLiteCommand(sql, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        public static void SaveDocument(
            string title,
            string content)
        {
            using (SQLiteConnection connection =
                   new SQLiteConnection(ConnectionString))
            {
                connection.Open();

                string sql =
                    @"INSERT INTO Documents
                      (Title, Content, CreatedAt)
                      VALUES (@title, @content, @createdAt);";

                using (SQLiteCommand command =
                       new SQLiteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue(
                        "@title",
                        title ?? string.Empty
                    );

                    command.Parameters.AddWithValue(
                        "@content",
                        content ?? string.Empty
                    );

                    command.Parameters.AddWithValue(
                        "@createdAt",
                        DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                    );

                    command.ExecuteNonQuery();
                }
            }
        }

        public static List<DocumentRecord> GetDocuments()
        {
            List<DocumentRecord> documents =
                new List<DocumentRecord>();

            using (SQLiteConnection connection =
                   new SQLiteConnection(ConnectionString))
            {
                connection.Open();

                string sql =
                    @"SELECT Id, Title, Content, CreatedAt
                      FROM Documents
                      ORDER BY Id DESC;";

                using (SQLiteCommand command =
                       new SQLiteCommand(sql, connection))
                using (SQLiteDataReader reader =
                       command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        DocumentRecord document =
                            new DocumentRecord();

                        document.Id = Convert.ToInt32(
                            reader["Id"]
                        );

                        document.Title =
                            Convert.ToString(reader["Title"]);

                        document.Content =
                            Convert.ToString(reader["Content"]);

                        document.CreatedAt =
                            Convert.ToString(reader["CreatedAt"]);

                        documents.Add(document);
                    }
                }
            }

            return documents;
        }
    }

    public class DocumentRecord
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string Content { get; set; }

        public string CreatedAt { get; set; }
    }
}
