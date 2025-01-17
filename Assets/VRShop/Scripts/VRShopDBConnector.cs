﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Data;
using Mono.Data.Sqlite;
using System.IO;

public static class VRShopDBConnector {

    private static readonly string APPLICATION_PATH = Directory.GetCurrentDirectory();
    private static readonly string ARTICLE_FOLDER_NAME = "Articles";
    private static readonly string DB_FILE_NAME = "vrshop.db";
    private static readonly string DATABASE_PATH = string.Format("URI=file:{0}/{1}/{2}", APPLICATION_PATH, ARTICLE_FOLDER_NAME, DB_FILE_NAME);

    public static readonly string ARTICLE_FOLDER_PATH = Path.Combine(APPLICATION_PATH, ARTICLE_FOLDER_NAME);

    private const string S_COL_ID = "id";
    private const string S_COL_NAME = "name";
    private const string S_COL_PRICE = "price";
    private const string S_COL_DESCRIPTION = "description";
    private const string S_COL_THUMBNAIL = "thumbnail";
    private const string S_COL_SIZE = "scale";

    private static readonly string ARTICLE_SEARCH_STRING_PLACEHOLDER = "@ArticleSearchString";
    private static readonly string ARTICLE_SEARCH_QUERY = string.Format(@"
        SELECT a.id, a.name, a.price, a.description, a.thumbnail, s.scale
            FROM
                tbl_articles a
            LEFT JOIN
                tbl_scale s
                    ON a.scale_factor = s.id
            WHERE
                a.name LIKE {0}
            OR
                a.category IN (
                    WITH parents AS (
                        SELECT id, name FROM tbl_categories
                            WHERE name LIKE {0}
                    )
                    SELECT c.id FROM tbl_categories c
                        JOIN parents
                            ON c.parent_id = parents.id
                    UNION
                        SELECT p.id FROM parents p
                )
            ORDER BY
                s.scale DESC, a.category DESC
    ", ARTICLE_SEARCH_STRING_PLACEHOLDER);

    public static List<VRShopArticle> SearchForArticle(string searchString) {
        // Prepare return list
        List<VRShopArticle> queriedArticles = new List<VRShopArticle>();

        // Prevent empty searches
        if (searchString.Length > 0) {
            // Connect to the SQLite DB
            SqliteConnection dbConnection = new SqliteConnection(DATABASE_PATH);
            dbConnection.Open();

            // Prepare the query using the search keyword
            SqliteCommand query = dbConnection.CreateCommand();
            query.CommandType = CommandType.Text;
            query.CommandText = ARTICLE_SEARCH_QUERY;
            query.Parameters.AddWithValue(ARTICLE_SEARCH_STRING_PLACEHOLDER, string.Format("%{0}%", searchString));

            // Exectute the query and read the results
            var reader = query.ExecuteReader();

            // Initialize column ordinals
            int colId = reader.GetOrdinal(S_COL_ID);
            int colName = reader.GetOrdinal(S_COL_NAME);
            int colPrice = reader.GetOrdinal(S_COL_PRICE);
            int colDescription = reader.GetOrdinal(S_COL_DESCRIPTION);
            int colThumbnail = reader.GetOrdinal(S_COL_THUMBNAIL);
            int colSize =  reader.GetOrdinal(S_COL_SIZE);
            
            // Iterate through every returned row
            while (reader.Read()) {
                int id;
                string articleName;
                decimal price;
                string description = "";
                byte[] img = null;
                float? size = null;

                // NOT NULL constraint applies, null check therefore not required
                id = reader.GetInt32(colId);
                articleName = reader.GetString(colName);
                price = reader.GetDecimal(colPrice);

                // May be null, needs to be caught
                if (!reader.IsDBNull(colDescription)) {
                    description = reader.GetString(colDescription);
                }

                if (!reader.IsDBNull(colThumbnail)) {
                    img = (byte[])reader[colThumbnail];
                }

                if (!reader.IsDBNull(colSize)) {
                    double dsize = reader.GetDouble(colSize);
                    size = (float)dsize;
                }

                // Add to result list
                VRShopArticle article = new VRShopArticle(id, price, articleName, description, img, size);
                queriedArticles.Add(article);
            }
            dbConnection.Close();
        }

        // Return results
        return queriedArticles;
    }
}
