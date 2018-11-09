using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Data;
using Mono.Data.Sqlite;

public static class VRShopDBConnector {

    private static readonly string DB_FILE_NAME = "vrshop.db";
    private static readonly int DB_ARTICLES_ID_COLUMN = 0;
    private static readonly int DB_ARTICLES_NAME_COLUMN = 1;
    private static readonly int DB_ARTICLES_PRICE_COLUMN = 2;
    private static readonly int DB_ARTICLES_DESCRIPTION_COLUMN = 3;

    private static readonly string ARTICLE_SEARCH_STRING_PLACEHOLDER = "@ArticleSearchString";
    private static readonly string ARTICLE_SEARCH_QUERY = string.Format(@"
        SELECT * FROM tbl_articles a
            WHERE a.name
                LIKE {0}
            OR a.category IN (
                WITH RECURSIVE rec_categories(parent) AS (
                    SELECT DISTINCT id
                        FROM tbl_categories c1
                        WHERE c1.category_name LIKE {0}
                    UNION ALL
                    SELECT c.parent_id 
                        FROM tbl_categories c, rec_categories r
                        WHERE c.id = r.parent
                )
                SELECT DISTINCT id
                    FROM tbl_categories c
                    WHERE c.id IN rec_categories
            )
            ORDER BY a.id DESC
    ", ARTICLE_SEARCH_STRING_PLACEHOLDER);

    public static IList<VRShopArticle> SearchForArticle(string searchString) {
        // Prepare return list
        IList<VRShopArticle> queriedArticles = new List<VRShopArticle>();

        // Prevent empty searches
        if (searchString.Length > 0) {
            // Connect to the SQLite DB
            string dbPath = string.Format("URI=file:{0}/{1}", Application.dataPath, DB_FILE_NAME);
            SqliteConnection dbConnection = new SqliteConnection(dbPath);
            dbConnection.Open();

            // Prepare the query using the search keyword
            SqliteCommand query = dbConnection.CreateCommand();
            query.CommandType = CommandType.Text;
            query.CommandText = ARTICLE_SEARCH_QUERY;
            query.Parameters.AddWithValue(ARTICLE_SEARCH_STRING_PLACEHOLDER, string.Format("%{0}%", searchString));

            // Exectute the query and read the results
            var reader = query.ExecuteReader();
            while (reader.Read()) {
                // DB-Schema: id | price | articleName | description
                int id = reader.GetInt32(DB_ARTICLES_ID_COLUMN);
                decimal price = reader.GetDecimal(DB_ARTICLES_PRICE_COLUMN);
                string articleName = reader.GetString(DB_ARTICLES_NAME_COLUMN);
                string description = reader.GetString(DB_ARTICLES_DESCRIPTION_COLUMN);

                // Add to result list
                VRShopArticle article = new VRShopArticle(id, price, articleName, description);
                queriedArticles.Add(article);
            }
            dbConnection.Close();
        }

        // Return results
        return queriedArticles;
    }
}
