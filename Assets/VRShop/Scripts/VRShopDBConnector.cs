using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Data;
using Mono.Data.Sqlite;

public static class VRShopDBConnector {

    private static readonly string DB_FILE_NAME = "vrshop.db";
    private const string S_COL_ID = "id";
    private const string S_COL_NAME = "name";
    private const string S_COL_PRICE = "price";
    private const string S_COL_DESCRIPTION = "description";
    private const string S_COL_THUMBNAIL = "thumbnail";
    private const string S_COL_SIZE = "model_size";
    private const string S_COL_ASSETBUNDLE = "asset_bundle";

    private static readonly string ARTICLE_SEARCH_STRING_PLACEHOLDER = "@ArticleSearchString";
    private static readonly string ARTICLE_SEARCH_QUERY = string.Format(@"
        SELECT a.id, a.name, a.price, a.description, a.thumbnail, a.model_size FROM tbl_articles a
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
            ORDER BY a.category DESC
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

            // Initialize column ordinals
            int colId = reader.GetOrdinal(S_COL_ID);
            int colName = reader.GetOrdinal(S_COL_NAME);
            int colPrice = reader.GetOrdinal(S_COL_PRICE);
            int colDescription = reader.GetOrdinal(S_COL_DESCRIPTION);
            int colThumbnail = reader.GetOrdinal(S_COL_THUMBNAIL);
            int colSize =  reader.GetOrdinal(S_COL_SIZE);
            int colAssetBundle = reader.GetOrdinal(S_COL_ASSETBUNDLE);
            
            // Iterate through every returned row
            while (reader.Read()) {
                int id;
                string articleName;
                decimal price;
                string description = "";
                byte[] img = null;
                float? size = null;
                string assetBundle = null;

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

                if (!reader.IsDBNull(colAssetBundle)) {
                    assetBundle = reader.GetString(colAssetBundle);
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
