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

Number 4 can still use some work. 

The app currently can 

1. Move folders
2. Pick new folders that were added with the file system 
	* These new folders need images in them before picking
3. Delete folders
4. Delete individual/single images

But needs to have

1. Rename folders
2. Add new folders with app function
3. Move individual or groups of images to folders
4. Re-scan a folder for newly images added

## Screen Shot

![Image](AppScreenShot4-2-25.png)


 


