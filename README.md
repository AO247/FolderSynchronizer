# FolderSynchronizer

The application periodically synchronizes two folders (source and a replica) ensuring the replica is an exact copy of the source. 
All file operations (creation, updates, deletion) are logged to both the console and a specified log file.


How to use:

**Firstly build**
$ dotnet run


**Run the application from the command line:**

The application requires 4 arguments:
1.  `source_path`: The full path to the source folder.
2.  `replica_path`: The full path to the replica folder.
3.  `interval`: The synchronization interval (e.g., `10s`, `5m`, `1h`).
4.  `log_file_path`: The full path for the log file.

Example:
$ dotnet run "D:\AA\source" "D:\AA\replica" 5s "D:\AA\log.txt"
