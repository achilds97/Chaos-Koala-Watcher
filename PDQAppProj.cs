/*
 * Coding Project for PDQ Application
 *
 * Alex Childs
 */

//Args: Directory, File Pattern

//Assumptions: 	All files given will be either ASCII or UTF-8 and will use windows line characters
//				File changes are not limited to one file at a time.
//				Needs to be able to handle absolute paths, relative paths, or UNC
//				Needs to use multi-threading to speed up runtime.
//				File names are case insensitive
//				Must run on Windows 10
//				File sizes will be up to 2 GB

//To start we need to grab the information of all of the files that match our file pattern in the given directory
//and store it in an array.

//Every 10 seconds check for any files that have been modified (date modified)\
//		To do this, either upate the array and sort it by date modified, then only check files until one comes out false
//		Alternatively just look at every file.
//		(Need to test these to see which method is fastest)

//If there are any new files we need to print out the file name and the number of lines.
//If there are any modified files we need to print out the file name and how ever many files were added/deleted
//If a file is deleted then we just need to output its name.

//TODO: Update this to use dictionaries so that you can check for if the file is there before doing a read on the file because that could
//		become very resource intense in some cases of this project. It will only do a read on a file if it is new or if it has been modified.
//		Otherwise the only thing that will be looked every time is the file names and the date modified values.
//TODO: Add comments throughout to explain methods and their functions.

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Timers;

namespace PDQAppProj {
	class ChaosKoalaWatcher {
		private Timer timer;

		string directoryPath 	{ get; set;}
		string filePattern		{ get; set;}
		string fileLength;
		string[,] fileInfoArray;
		string[,] currFileContents;
		ArrayList fileArray		= new ArrayList();
		Dictionary<string, string[]> fileDict = new Dictionary<string, string[]>();

		static void Main(){
			ChaosKoalaWatcher chimp = new ChaosKoalaWatcher();
			chimp.getDirectoryInfo();

			chimp.init();
			chimp.run();
		}

		public ChaosKoalaWatcher() {
			string directoryPath = "";
			string filePattern = "";
		}

		void init() {
			string[] stringArray = Directory.GetFiles(@directoryPath, filePattern);
			ArrayList tempArray = new ArrayList(stringArray);
			fileInfoArray = checkFiles(tempArray);

			Console.WriteLine("Initial contents of directory:");

			fileDict = getAllFileInfo(tempArray);
			fileLength = (-1 * (getMaxChar(tempArray))).ToString();

			Console.WriteLine("{0, "+ fileLength + "} {1, "+ fileLength + "}", "File Name", "Num Lines");
			foreach(KeyValuePair<string, string[]> item in fileDict) {
				Console.WriteLine("{0, "+ fileLength + "} {1, "+ fileLength + "}", getFileNameFromPath(item.Key), item.Value[1]);
			}
			Console.WriteLine();
		}

		void run() {
			timer = new System.Timers.Timer();
			timer.Interval = 10000;

			timer.Elapsed += OnTimedEvent;
			timer.AutoReset = true;
			timer.Enabled = true;

			Console.WriteLine("Running the Chaos Koala Watcher. Press enter to stop execution");
			Console.ReadLine();
		}

		void OnTimedEvent(Object source, System.Timers.ElapsedEventArgs e) {
			checkDirectory();
		}

		void getDirectoryInfo() {
			Console.WriteLine("Please enter a file directory");
			directoryPath = Console.ReadLine();
			Console.WriteLine("Please enter a file pattern: ");
			filePattern = Console.ReadLine();
		}

		void checkDirectory() {
			int numLines;
			string[] infoArray = new string[2];
			ArrayList newFilesArray = new ArrayList();
			ArrayList modifiedFilesArray = new ArrayList();
			ArrayList deletedFilesArray = new ArrayList();
			ArrayList itemsToBeRemoved = new ArrayList();

			string[] stringArray = Directory.GetFiles(@directoryPath, filePattern);
			ArrayList tempArray = new ArrayList(stringArray);
			currFileContents = checkFiles(tempArray);

			//New Files
			for (int i = 0; i < currFileContents.Length / 3; i++) {
				if(fileDict.ContainsKey((string) currFileContents[i,0]) == false) {
					numLines = countLines(currFileContents[i,0]);

					infoArray = new string[2];
					infoArray[0] = currFileContents[i,1];
					infoArray[1] = numLines.ToString();
					newFilesArray.Add(new string[] {currFileContents[i,0], numLines.ToString()});

					fileDict.Add(currFileContents[i,0], infoArray);

					fileLength = (-1 * (getMaxChar(tempArray))).ToString();
				}
			}

			//Modified Files
			for (int i = 0; i < currFileContents.Length / 3; i++) {
				if(fileDict.ContainsKey((string) currFileContents[i,0]) == true) {
					if((fileDict[currFileContents[i,0]][0] == currFileContents[i,1]) == false) {
						numLines = countLines(currFileContents[i,0]);

						infoArray = new string[2];
						infoArray[0] = currFileContents[i,1];
						infoArray[1] = numLines.ToString();
						modifiedFilesArray.Add(new string[] {currFileContents[i,0], (numLines - Int32.Parse(fileDict[currFileContents[i,0]][1])).ToString()});

						fileDict[currFileContents[i,0]] = infoArray;
					}
				}
			}

			foreach (KeyValuePair<string, string[]> item in fileDict) {
				if(contains(currFileContents, item.Key) == false) {
					deletedFilesArray.Add(getFileNameFromPath(item.Key));
					itemsToBeRemoved.Add(item.Key);

					fileLength = (-1 * (getMaxChar(tempArray))).ToString();
				}
			}

			foreach(string item in itemsToBeRemoved) {
				fileDict.Remove(item);
			}

			if(newFilesArray.Count > 0) {
				printNewFiles(newFilesArray);
				Console.WriteLine();
			}

			if(modifiedFilesArray.Count > 0) {
				printModifiedFiles(modifiedFilesArray);
				Console.WriteLine();
			}

			if(deletedFilesArray.Count > 0) {
				printDeletedFiles(deletedFilesArray);
				Console.WriteLine();
			}
		}

		string[,] checkFiles(ArrayList fileArray) {

			string[,] fileInfo = new string[fileArray.Count,3];

			for(int i=0; i < fileArray.Count; i++) {
				fileInfo[i,0] = (string) fileArray[i];
				fileInfo[i,1] = File.GetLastWriteTime(fileInfo[i,0]).ToString("dd MM yyy hh:mm:ss tt");
			}

			return fileInfo;
		}

		Dictionary<string, string[]> getAllFileInfo(ArrayList fileArray) {
			string[] fileInfo = new string[2];
			string fileName;
			Dictionary<string, string[]> fileDictionary = new Dictionary<string, string[]>();

			for(int i=0; i < fileArray.Count; i++) {
				fileName = (string) fileArray[i];
				fileInfo = new string[2];
				fileInfo[0] = File.GetLastWriteTime(fileName).ToString("dd MM yyy hh:mm:ss tt");
				fileInfo[1] = countLines(fileName).ToString();
				fileDictionary.Add(fileName, fileInfo);
			}

			return fileDictionary;
		}

		string getFileNameFromPath(string filePath) {
			int lastIndex = filePath.LastIndexOf("\\", filePath.Length);
			int fileNameLength = filePath.Length - lastIndex;

			return filePath.Substring(lastIndex + 1, fileNameLength - 1);
		}

		void printNewFiles(ArrayList fileArray) {
			Console.WriteLine("NEW FILES: ");
			Console.WriteLine("{0, "+ fileLength + "} {1, "+ fileLength + "}", "File Name", "Number  of Lines");
			foreach(string[] arr in fileArray) {
				Console.WriteLine("{0, "+ fileLength + "} {1, "+ fileLength + "}", getFileNameFromPath(arr[0]), arr[1]);
			}
		}

		void printModifiedFiles(ArrayList fileArray) {
			Console.WriteLine("MODIFIED FILES: ");
			Console.WriteLine("{0, "+ fileLength + "} {1, "+ fileLength + "}", "File Name", "Number of Lines Changed");
			foreach(string[] arr in fileArray) {
				Console.WriteLine("{0, "+ fileLength + "} {1, "+ fileLength + "}", getFileNameFromPath(arr[0]), arr[1]);
			}
		}

		void printDeletedFiles(ArrayList fileArray) {
			Console.WriteLine("DELETED FILES: ");

			foreach(Object obj in fileArray) {
				Console.WriteLine((string) obj);
			}
		}

		bool contains(String[,] fileArray, string fileName) {
			foreach(string name in fileArray) {
				if(name == fileName) {
					return true;
				}
			}
			return false;
		}

		bool contains(String[] fileArray, string fileName) {
			foreach(string name in fileArray) {
				if(name == fileName) {
					return true;
				}
			}

			return false;
		}

		int getIndex(String[,] fileArray, string fileName) {

			for(int i = 0; i < fileArray.Length / 3; i++) {
				if(fileArray[i,0] == fileName) {
					return i;
				}
			}

			return -1;
		}

		int countLines(string fileName) {
			int numLines = 0;

			using (var input = File.OpenText(fileName)) {
				while (input.ReadLine() != null) {
					++numLines;
				}
			}

			return numLines;
		}

		int getMaxChar(ArrayList fileNames) {
			int numChars = 0;

			foreach(string file in fileNames) {
				if(file.Length > numChars) {
					numChars = file.Length;
				}
			}

			return numChars;
		}

	}
}
