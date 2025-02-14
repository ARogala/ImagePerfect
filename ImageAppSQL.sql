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
  `FolderTags` Varchar(2000),
  `FolderRating` tinyint unsigned,
  `HasFiles` Bool,
  `IsRoot` Bool,
  `FolderContentMetaDataScanned` Bool,
  `AreImagesImported` Bool,
	PRIMARY KEY (`FolderId`),
	FULLTEXT KEY `fulltext` (`FolderName`,`FolderPath`, `FolderDescription`, `FolderTags`)
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
	`ImageTags` Varchar(2000),
	`ImageRating` tinyint unsigned,
	`ImageFolderPath` Varchar(2000) NOT NULL,
	`ImageMetaDataScanned` Bool,
	`FolderId` bigint unsigned DEFAULT NULL,
	PRIMARY KEY (`ImageId`),
	KEY `FolderId` (`FolderId`),
	FULLTEXT KEY `fulltext` (`ImagePath`, `ImageTags`),
	CONSTRAINT `images_ibfk_1` FOREIGN KEY (`FolderId`) REFERENCES `folders` (`FolderId`) ON DELETE CASCADE ON UPDATE CASCADE
);


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