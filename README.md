## Image Perfect

## About
Image Perfect is a cross platform Linux (Ubuntu tested) and Windows photo manager application. It is written with C#, MySQL, and Avalonia UI framework. The MVVM pattern is used. The Materialized Path Technique was used to model hierarchical data of the folder structure in the file system.

A few other major dependencies are 
1. Dapper -- SQL ORM
2. CsvHelper
3. SixLabors Image Sharp
4. Any developer can view the rest in Visual Studio.


I wrote this application to learn some desktop application development and because I felt the current photo management applications on the market did not fit my needs. In particular many photo managers seem to have small thumbnails for the image. That makes it harder to organize and decide which ones to delete and which ones are your favorites. Shotwell on Linux is actually pretty good but importing new images gets really slow with large libraries. Primarily Image Perfect was written with several things in mind. 

1. Big thumbnails 500px - 600px.
	* Image width can be adjusted from 300px all the way up to 600px!!
	* Images are displayed on the fly and no thumbnails are written to disk.
2. Good tagging system for both the photos themselves and the folders they are in.
	* Image tags and rating are written on the image file itself as well as stored in the database. 
	* Folder tags, description, and rating are only stored in the database.
	* A cover image for the folders can also be selected.
	* Tags can only be added and removed one at a time.
3. Perform well with large libraries no waiting hours on end to import new photos.
	* I did this by using MySqlBulkLoader to insert data from a csv file.
	* Note that there is no library monitoring like other apps provide. This means you have to keep track of what new folders you want to add after the initial library import.
	* Also photos and metadata are not imported initially but are done on each folder by the user before before viewing the photos in that folder. The operations are fast enough this is not a issue for me. 
4. Model the file system to make it easy and fast to move folders/images in the application while moving the folders/images in the file system at the same time as well.
5. Provide a way to import all the tags written on the image from Shotwell (this requires you had Shotwell actually write the tags and rating to the image itself).
6. ImagePerfect is currently designed to be a great image viewer with tagging and organization features. Its use case for me is to get a big library organized and cleaned up/trimmed down.

Number 4 is basically complete but could maybe use some fine tuning. 

The app currently can 

1. Move folders
2. Pick new folders that were added with the file system 
	* These new folders should have images in them before picking
	* Otherwise best just to create a new empty folder within the app and move images to it 
3. Create new empty folders within the app
4. Delete folders
5. Delete individual/single images
6. Delete multiple images
7. Move multiple images

Maybe add

1. Rename folders
2. Rename images
3. Re-scan a folder for newly images added -- useful so you can add images to a folder from they file system then re-scan

## Other Features to add

1. A way to find duplicate images -- big task
2. Facial recognition -- big task
3. Improve the UI
4. Improve Tagging right now you can only add one tag at a time. Make it so you can add several tags at once if comma separated. Same with remove you can only remove one at a time. 
5. Maybe make a SQLite version so no server set up -- the reason for the MySQL server is i plan to have a mobile client so you can at least view photos via you phone
6. look into making the mobile client 
7. Maybe find a way to scan library root folder to find folders added to file system but not added to app
8. A way to prevent adding the same folder twice. Right now its up to the user to be careful about adding new folders after the initial library add.
9. Improve image move so you can move images to a new folder even if the new folder contains images with the same name. 
10. Scan and import multiple folders at once or in batches. 

## Screen Shot

![Image](AppScreenShot4-2-25.png)

## Build and Install directions

to be added

## Server set up and configuration directions

to be added

## Back up and restore MySQL database on Windows

We will use mysqldump command to do this. Note: ImagePerfect stores references to your image files in the database so when you back up/restore the image files should be stored in the exact same location as they were before you needed to restore. So if your images were in C:\Users\username\Pictures they must be there and in all their same folders for a restore to work.

1. To back up/restore use Windows command prompt NOT power shell!!!
2. First open command prompt in C:\Program Files\MySQL\MySQL Server 8.0\bin
	* Open command prompt and type: 
	* cd C:\Program Files\MySQL\MySQL Server 8.0\bin
3. To back-up type: 
	* mysqldump -u root -p imageperfect > C:\MySQLBackup\imageperfect_YYYY_MM_DD.sql
	* It will ask for your root server password after hitting enter.
	* Your back up sql file will now be in C:\MySQLBackup check and ensure it is there.
4. To restore type:
	* mysql -u root -p imageperfect < C:\MySQLBackup\imageperfect_YYYY_MM_DD.sql
	* It will ask for your root server password after hitting enter.
	* Your database should now be restored.
	
NOTE: It would be best to try this before spending too much time organizing your photos in the app. Make sure you can back up before wasting time. Its easy to spend hours adding cover images, tags, and notes about the event/day

## Back up and restore MySQL database on Ubuntu

This is basically the same as Windows

1. Open terminal in the location/folder you want your backup file.
2. To backup type: 
	* sudo mysqldump imageperfect > imageperfect_YYYY_MM_DD.sql
	* Ubuntu will ask for your root password after hitting enter.
	* This will dump the imageperfect database in the backup location.
	
3. To restore type:
	* sudo mysql imageperfect < imageperfect_YYYY_MM_DD.sql
	* Ubuntu will ask for your root password after hitting enter.
	* Your database should now be restored.
	* Obvious or maybe not, but terminal should be opened in the location/folder where your backup file is located for the restore to work.


## Directions to back up photos and move the app and database to a new computer.

Basically set up you new computer. Build the app, set up the server, and run the backup commands above. Just note that the images should be in the exact same location as before. The drive name should be the same as well. Can't backup the database for drive C: and expect the restore to work if all your images are in drive D: after a new computer restore.

Note: ImagePerfect stores references to your image files in the database so when you back up/restore the image files should be stored in the exact same location as they were before you needed to restore. So if your images were in C:\Users\username\Pictures they must be there and in all their same folders for a restore to work.

Same idea on Ubuntu systems.

## User guide

to be added




 


