/*
ubuntu mysql user is imageperfect with the same pw thats in settings for windows
*/

CREATE DATABASE imageperfect;
/*
PARENT table

I am using the Materialized Path Technique to model hierarchical data

https://dzone.com/articles/materialized-paths-tree-structures-relational-database

https://stackoverflow.com/questions/1437066/mysql-fulltext-search-and-like

Bool is tinyint(1) 1 = true 0 = false

*/
CREATE TABLE `folders` (
	`FolderId` bigint unsigned NOT NULL AUTO_INCREMENT,
	`FolderName` Varchar(200) NOT NULL,
  	`FolderPath` Varchar(2000) NOT NULL,
  	`HasChildren` Bool,
	`CoverImagePath` Varchar(2000),
	`FolderDescription` Varchar(3000),
	`FolderRating` tinyint unsigned,
	`HasFiles` Bool,
	`IsRoot` Bool,
	`FolderContentMetaDataScanned` Bool,
	`AreImagesImported` Bool,
	PRIMARY KEY (`FolderId`),
	FULLTEXT KEY `fulltext` (`FolderName`,`FolderPath`, `FolderDescription`)
);

/*
CHILD Table
https://stackoverflow.com/questions/1481476/when-to-use-on-update-cascade

images is the child of folders

one image can have only 1 folder but 1 folder can have many images ONE TO MANY

ON DELETE CASCADE to delete all images if a folder is deleted 
*/

CREATE TABLE `images` (
	`ImageId` bigint unsigned NOT NULL AUTO_INCREMENT,
	`ImagePath` Varchar(2000) NOT NULL,
	`FileName` Varchar(500) NOT NULL,
	`ImageRating` tinyint unsigned,
	`ImageFolderPath` Varchar(2000) NOT NULL,
	`ImageMetaDataScanned` Bool,
	`FolderId` bigint unsigned DEFAULT NULL,
	PRIMARY KEY (`ImageId`),
	KEY `FolderId` (`FolderId`),
	FULLTEXT KEY `fulltext` (`ImagePath`),
	CONSTRAINT `images_ibfk_1` FOREIGN KEY (`FolderId`) REFERENCES `folders` (`FolderId`) ON DELETE CASCADE ON UPDATE CASCADE
);

CREATE TABLE `tags`(
	`TagId` bigint unsigned NOT NULL AUTO_INCREMENT,
	`TagName` Varchar(100) NOT NULL,
	PRIMARY KEY (`TagId`),
	CONSTRAINT `tags_uq` UNIQUE (`TagName`)
);

CREATE TABLE `folder_tags_join`(
	`FolderId` bigint unsigned NOT NULL,
	`TagId` bigint unsigned NOT NULL,
	PRIMARY KEY (`FolderId`, `TagId`),
	CONSTRAINT `folder_tags_join_idfk_1` FOREIGN KEY (`FolderId`) REFERENCES `folders` (`FolderId`) ON DELETE CASCADE ON UPDATE CASCADE,
	CONSTRAINT `folder_tags_join_idfk_2` FOREIGN KEY (`TagId`) REFERENCES `tags` (`TagId`) ON DELETE CASCADE ON UPDATE CASCADE
);

/*
ON DELETE CASCADE to delete all image_tags_join when an image is deleted
ON DELETE CASCADE to delete all image_tags_join when an tag is deleted
*/
CREATE TABLE `image_tags_join`(
	`ImageId` bigint unsigned NOT NULL,
	`TagId` bigint unsigned NOT NULL,
	PRIMARY KEY (`ImageId`, `TagId`),
	CONSTRAINT `image_tags_join_ibfk_1` FOREIGN KEY (`ImageId`) REFERENCES `images` (`ImageId`) ON DELETE CASCADE ON UPDATE CASCADE,
	CONSTRAINT `image_tags_join_ibfk_2` FOREIGN KEY (`TagId`) REFERENCES `tags` (`TagId`) ON DELETE CASCADE ON UPDATE CASCADE

);

/*
https://stackoverflow.com/questions/4715183/how-can-i-ensure-that-there-is-one-and-only-one-row-in-db-table
enum to ensure a single row settings table.
*/
CREATE TABLE `settings` (
	`SettingsId` enum('1') NOT NULL,
	`MaxImageWidth` int unsigned NOT NULL,
	`FolderPageSize` int unsigned NOT NULL,
	`ImagePageSize` int unsigned NOT NULL,
	PRIMARY KEY (`SettingsId`)
);

INSERT INTO settings (MaxImageWidth, FolderPageSize, ImagePageSize) VALUES (500, 20, 60); 
UPDATE settings SET MaxImageWidth = 550 WHERE SettingsId = 1;

CREATE TABLE `folder_saved_favorites` (
	`SavedId` bigint unsigned NOT NULL AUTO_INCREMENT,
	`FolderId` bigint unsigned,
	PRIMARY KEY (`SavedId`),
	CONSTRAINT `folderid_uq` UNIQUE (`FolderId`)
);

CREATE TABLE `saved_directory` (
	`SavedDirectoryId` enum('1') NOT NULL,
	`SavedDirectory` Varchar(2000) NOT NULL,
	`SavedFolderPage` int unsigned NOT NULL,
	`SavedTotalFolderPages` int unsigned NOT NULL,
	`SavedImagePage` int unsigned NOT NULL,
	`SavedTotalImagePages` int unsigned NOT NULL,
	`XVector` double NOT NULL,
	`YVector` double NOT NULL,
	PRIMARY KEY (`SavedDirectoryId`)
);

INSERT INTO saved_directory (SavedDirectory, SavedFolderPage, SavedTotalFolderPages, SavedImagePage, SavedTotalImagePages, XVector, YVector) VALUES ("",1,1,1,1,0,0);

/*get all tags for image*/
SELECT * FROM images 
JOIN image_tags_join ON image_tags_join.ImageId = images.ImageId
JOIN tags ON image_tags_join.TagId = tags.TagId WHERE images.ImageId = 1;

/*Get all images whith tag = Tree*/
SELECT * FROM images 
JOIN image_tags_join ON image_tags_join.ImageId = images.ImageId
JOIN tags ON image_tags_join.TagId = tags.TagId WHERE tags.TagName = "Tree";

/*get all tags for folder*/
SELECT TagName FROM folders
	JOIN folder_tags_join ON folder_tags_join.FolderId = folders.FolderId
	JOIN tags ON folder_tags_join.TagId = tags.TagId WHERE folders.FolderId = 1;

SELECT * FROM folders 
	JOIN folder_tags_join ON folder_tags_join.FolderId = folders.FolderId
	JOIN tags ON folder_tags_join.TagId = tags.TagId WHERE REGEXP_LIKE(folders.FolderPath, 'C:\\\\Users\\\\arogala\\\\Documents\\\\CSharp\\\\SamplePicsApp\\\\[^\\\\]+\\\\?$') ORDER BY folders.FolderName;

SELECT tags.TagId, tags.TagName, folders.FolderId FROM folders
	JOIN folder_tags_join ON folder_tags_join.FolderId = folders.FolderId
	JOIN tags ON folder_tags_join.TagId = tags.TagId WHERE folders.IsRoot = 1;


/*
https://stackoverflow.com/questions/66848547/mysql-error-code-3948-loading-local-data-is-disabled-this-must-be-enabled-on-b
https://bugs.mysql.com/bug.php?id=91872
https://stackoverflow.com/questions/37343336/how-to-save-file-path-in-the-mysql-database-using-string-in-c-sharp

https://mysqlconnector.net/troubleshooting/load-data-local-infile/

SET PERSIST local_infile = 1;
*/
LOAD DATA LOCAL INFILE "C:/Users/arogala/Documents/CSharp/DirectoryApp/folders.csv" INTO TABLE folders
FIELDS TERMINATED BY ',' 
ENCLOSED BY '"'
LINES TERMINATED BY '\r\n'
IGNORE 1 LINES;

DELETE FROM folders;

ALTER TABLE folders AUTO_INCREMENT = 1;
/*https://stackoverflow.com/questions/32155447/mysql-regexp-matching-only-subdirectories-of-given-directory  
	No clue how the hell that works but it produces the needed result (gets all folders in SamplePictures but no sub directories)

SELECT * FROM folders WHERE REGEXP_LIKE(FolderPath, 'C:\\\\Users\\\\arogala\\\\Documents\\\\CSharp\\\\SamplePictures\\\\[^\\\\]+\\\\?$');

This query selects all rows from the folders table where the FolderPath matches a specific folder path pattern using a regular expression.

Regex Pattern:
C:\\Users\\arogala\\Documents\\CSharp\\SamplePictures\\[^\\]+\\?$
Because it's inside a SQL string, the backslashes are escaped twice:

\\ becomes \ in regex

\\\\ in SQL = \\ in regex

So the actual regex the engine sees is:
C:\Users\arogala\Documents\CSharp\SamplePictures\[^\\]+\\?$

	| Segment                                             | Meaning                                                                                   |
| --------------------------------------------------- | ----------------------------------------------------------------------------------------- |
| `C:\Users\arogala\Documents\CSharp\SamplePictures\` | Matches the **exact full folder path** up to `SamplePictures\`                            |
| `[^\\]+`                                            | Match **one or more characters that are NOT backslashes** (i.e., a single subfolder name) |
| `\\?`                                               | An **optional backslash** (0 or 1 occurrence)                                             |
| `$`                                                 | Anchors the match to the **end of the string**                                            |

The $ ensures the match stops after one subfolder, and nothing comes after.

the $ in a regular expression is a special character called an anchor, and it means: “Match only if this is the end of the string.”



In plain English
Match folder paths that:

Start with
C:\Users\arogala\Documents\CSharp\SamplePictures\

Are followed by one folder name (no slashes in it)

May or may not end in a trailing backslash

No further subfolders after that

Example Matches
✅ Will Match:

C:\Users\arogala\Documents\CSharp\SamplePictures\Cats

C:\Users\arogala\Documents\CSharp\SamplePictures\Cats\

❌ Won't Match:

C:\Users\arogala\Documents\CSharp\SamplePictures\Cats\SubFolder

C:\Users\arogala\Documents\CSharp\SamplePictures\ (because no folder after it)

Why Use \\? at the End?
It allows the regex to match both forms of the path:

With trailing backslash (\)

Without it

*/
SELECT * FROM folders WHERE REGEXP_LIKE(FolderPath, 'C:\\\\Users\\\\arogala\\\\Documents\\\\CSharp\\\\SamplePictures\\\\[^\\\\]+\\\\?$');



SELECT * FROM folders WHERE MATCH (`FolderName`,`FolderPath`,`FolderDescription`,`FolderTags`) AGAINST ('C:\\\\Users\\\\arogala\\\\Documents\\\\CSharp\\\\SamplePictures\\\\' IN BOOLEAN MODE);

SELECT * FROM folders WHERE FolderPath LIKE 'C:\\\\Users\\\\arogala\\\\Documents\\\\CSharp\\\\SamplePictures\\\\space%';

SELECT * FROM folders WHERE FolderPath LIKE 'C:\\\\Users\\\\arogala\\\\Documents\\\\CSharp\\\\SamplePictures\\\\%';

SELECT * FROM folders WHERE REGEXP_LIKE(FolderPath, 'C:\\\\Users\\\\arogala\\\\Documents\\\\CSharp\\\\SamplePictures\\\\[a-z]\\\\*');

SELECT * FROM folders WHERE REGEXP_LIKE(FolderPath, 'C:\\\\Users\\\\arogala\\\\Documents\\\\CSharp\\\\SamplePictures\\\\');

SELECT * FROM folders WHERE REGEXP_LIKE(FolderPath, 'C:\\\\Users\\\\arogala\\\\Documents\\\\CSharp\\\\SamplePictures\\\\[^\\\\]+\\\\?$');

SELECT * FROM folders WHERE REGEXP_LIKE(FolderPath, 'C:\\\\Users\\\\arogala\\\\Documents\\\\CSharp\\\\SamplePictures\\\\space');

/*
https://stackoverflow.com/questions/12754470/mysql-update-case-when-then-else
https://stackoverflow.com/questions/13673890/mysql-case-to-update-multiple-columns
*/

UPDATE folders SET FolderPath = CASE 
	WHEN FolderId = 11 THEN 'C:\\Users\\arogala\\Documents\\CSharp\\SamplePictures\\Dad\\space'
	WHEN FolderId = 13 THEN 'C:\\Users\\arogala\\Documents\\CSharp\\SamplePictures\\Dad\\space\\fav1'
	WHEN folderId = 14 THEN 'C:\\Users\\arogala\\Documents\\CSharp\\SamplePictures\\Dad\\space\\fav1\\OtherStuff'
	WHEN folderId = 15 THEN 'C:\\Users\\arogala\\Documents\\CSharp\\SamplePictures\\Dad\\space\\fav1\\OtherStuff\\OtherStuffTwo'
	ELSE FolderPath
  END,
CoverImagePath = CASE
	WHEN FolderId = 11 THEN 'C:\\Users\\arogala\\Documents\\CSharp\\SamplePictures\\Dad\\space\\hs-1995-01-a-1280_wallpaper.jpg'
	WHEN FolderId = 13 THEN 'C:\\Users\\arogala\\Documents\\CSharp\\SamplePictures\\Dad\\space\\fav1\\ryan-hutton-37733-unsplash.jpg'
	WHEN folderId = 14 THEN 'C:\\Users\\arogala\\Documents\\CSharp\\SamplePictures\\Dad\\space\\fav1\\OtherStuff\\wp5.jpg'
	WHEN folderId = 15 THEN 'C:\\Users\\arogala\\Documents\\CSharp\\SamplePictures\\Dad\\space\\fav1\\OtherStuff\\OtherStuffTwo\\robson-hatsukami-morgan-296510-unsplash.jpg'
  ELSE CoverImagePath
  END
WHERE FolderId IN (11,13,14,15);

/*
https://stackoverflow.com/questions/64194596/mysql-select-distinct-values-from-a-column-where-values-are-separated-by-comma
https://stackoverflow.com/questions/49196949/get-all-distinct-values-from-a-particular-column-with-comma-separated-values
*/
with recursive 
    data as (select concat(ImageTags, ',') rest from images),
    words as (
        select substring(rest, 1, locate(',', rest) - 1) word, substring(rest, locate(',', rest) + 1) rest
        from data
        union all
        select substring(rest, 1, locate(',', rest) - 1) word, substring(rest, locate(',', rest) + 1) rest
        from words
        where locate(',', rest) > 0
)
select distinct word from words order by word



/*From table inspect 6-3-25*/

CREATE TABLE `folders` (
  `FolderId` bigint unsigned NOT NULL AUTO_INCREMENT,
  `FolderName` varchar(200) NOT NULL,
  `FolderPath` varchar(2000) NOT NULL,
  `HasChildren` tinyint(1) DEFAULT NULL,
  `CoverImagePath` varchar(2000) DEFAULT NULL,
  `FolderDescription` varchar(3000) DEFAULT NULL,
  `FolderRating` tinyint unsigned DEFAULT NULL,
  `HasFiles` tinyint(1) DEFAULT NULL,
  `IsRoot` tinyint(1) DEFAULT NULL,
  `FolderContentMetaDataScanned` tinyint(1) DEFAULT NULL,
  `AreImagesImported` tinyint(1) DEFAULT NULL,
  PRIMARY KEY (`FolderId`),
  FULLTEXT KEY `fulltext` (`FolderName`,`FolderPath`,`FolderDescription`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci

CREATE TABLE `images` (
  `ImageId` bigint unsigned NOT NULL AUTO_INCREMENT,
  `ImagePath` varchar(2000) NOT NULL,
  `FileName` varchar(500) NOT NULL,
  `ImageRating` tinyint unsigned DEFAULT NULL,
  `ImageFolderPath` varchar(2000) NOT NULL,
  `ImageMetaDataScanned` tinyint(1) DEFAULT NULL,
  `FolderId` bigint unsigned DEFAULT NULL,
  PRIMARY KEY (`ImageId`),
  KEY `FolderId` (`FolderId`),
  FULLTEXT KEY `fulltext` (`ImagePath`),
  CONSTRAINT `images_ibfk_1` FOREIGN KEY (`FolderId`) REFERENCES `folders` (`FolderId`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci

CREATE TABLE `tags` (
  `TagId` bigint unsigned NOT NULL AUTO_INCREMENT,
  `TagName` varchar(100) NOT NULL,
  PRIMARY KEY (`TagId`),
  UNIQUE KEY `tags_uq` (`TagName`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci

CREATE TABLE `folder_tags_join` (
  `FolderId` bigint unsigned NOT NULL,
  `TagId` bigint unsigned NOT NULL,
  PRIMARY KEY (`FolderId`,`TagId`),
  KEY `folder_tags_join_idfk_2` (`TagId`),
  CONSTRAINT `folder_tags_join_idfk_1` FOREIGN KEY (`FolderId`) REFERENCES `folders` (`FolderId`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `folder_tags_join_idfk_2` FOREIGN KEY (`TagId`) REFERENCES `tags` (`TagId`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci

CREATE TABLE `image_tags_join` (
  `ImageId` bigint unsigned NOT NULL,
  `TagId` bigint unsigned NOT NULL,
  PRIMARY KEY (`ImageId`,`TagId`),
  KEY `image_tags_join_ibfk_2` (`TagId`),
  CONSTRAINT `image_tags_join_ibfk_1` FOREIGN KEY (`ImageId`) REFERENCES `images` (`ImageId`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `image_tags_join_ibfk_2` FOREIGN KEY (`TagId`) REFERENCES `tags` (`TagId`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci

CREATE TABLE `settings` (
  `SettingsId` enum('1') NOT NULL,
  `MaxImageWidth` int unsigned NOT NULL,
  `FolderPageSize` int unsigned NOT NULL,
  `ImagePageSize` int unsigned NOT NULL,
  PRIMARY KEY (`SettingsId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci

INSERT INTO settings (MaxImageWidth, FolderPageSize, ImagePageSize) VALUES (500, 20, 60);

CREATE TABLE `folder_saved_favorites` (
  `SavedId` bigint unsigned NOT NULL AUTO_INCREMENT,
  `FolderId` bigint unsigned DEFAULT NULL,
  PRIMARY KEY (`SavedId`),
  UNIQUE KEY `folderid_uq` (`FolderId`)
) ENGINE=InnoDB AUTO_INCREMENT=19 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci