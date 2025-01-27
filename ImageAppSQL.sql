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
*/
LOAD DATA LOCAL INFILE "C:/Users/arogala/Documents/CSharp/DirectoryApp/folders.csv" INTO TABLE folders
FIELDS TERMINATED BY ',' 
ENCLOSED BY '"'
LINES TERMINATED BY '\r\n'
IGNORE 1 LINES;

DELETE FROM folders;

ALTER TABLE folders AUTO_INCREMENT = 1;