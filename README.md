## Image Perfect

## About
Image Perfect is a cross platform Linux (Ubuntu tested) and Windows photo manager application. It is written with C#, MySQL, and Avalonia UI framework. The MVVM pattern is used. The Materialized Path Technique was used to model hierarchical data of the folder structure in the file system.

I wrote this application to learn some desktop application development and because I felt the current photo management applications on the market did not fit my needs. In particular many photo managers seem to have small thumbnails for the image. Shotwell on Linux is actually pretty good but importing new images gets really slow with large libraries. on This makes it hard to view your and organize your photos. Primarily Image Perfect was written with several things in mind. 

1. Big thumbnails 500px - 600px.
2. Good tagging system for both the photos themselves and the folders they are in.
3. Perform well with large libraries no waiting hours on end to import new photos.
4. Model they file system make it easy and fast to move folders in the application while moving the folders in the file system at the same times as well.
5. Provide a way to import all the tags written on the image from Shotewll (this requires you had Shotwell actually write the tags and rating to the image itself).

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
4. Re-scan a folder for new images added


 


