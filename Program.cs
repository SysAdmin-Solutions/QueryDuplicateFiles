using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

class QueryDuplicateFileNames
{
    public static bool Continue { get; set; }
    static void Main(string[] args)
    {
        Continue = true;
        while (Continue == true)
            {
                Program();
            Console.WriteLine(@"Would you like to run another query? Press 'n' to exit the program.");
            string ContinueQuery = Console.ReadLine();
            Console.Clear();
            if (ContinueQuery.ToString().ToLower() == "n")
            {
                Continue = false;
            }
            else
            {
                Continue = true;
            }
            };
        
    }
    public static string rootSearchFolder { get; set; }

    public static void Program()
    {
        try
        {
            // Uncomment QueryDuplicates2 to run that query.  
            QueryDuplicates();
            // QueryDuplicates2();  

            // Keep the console window open in debug mode.  
        }
        catch (Exception ex) { Console.WriteLine("Query was unable to finish. An error has occured: \n " + ex); }

    }

    static void QueryDuplicates()
    {
        // Change the root drive or folder if necessary  
        Console.WriteLine("Type the Location of the root Folder you would like to query...");
        rootSearchFolder = Console.ReadLine();
        var startFolder = rootSearchFolder;
        Console.WriteLine("Running Query for duplicate file names with the root location of " + startFolder );
        Console.WriteLine("Please be patient, as this may take a while...");

        // Take a snapshot of the file system.  
        System.IO.DirectoryInfo dir = new System.IO.DirectoryInfo(startFolder);

        // This method assumes that the application has discovery permissions  
        // for all folders under the specified path.  
        IEnumerable<System.IO.FileInfo> fileList = dir.GetFiles("*.*", System.IO.SearchOption.AllDirectories);

        // used in WriteLine to keep the lines shorter  
        int charsToSkip = startFolder.Length;

        // var can be used for convenience with groups. 
            var queryDupNames =
                from file in fileList
                where (file.FullName.Contains("pdf") || (file.FullName.Contains("dwg")) || (file.FullName.Contains("slddrw")))
                group file.FullName.Substring(charsToSkip) by (file.Name.Substring(0, (file.Name.Length - file.Extension.Length) )) into fileGroup
                //group file.FullName.Substring(charsToSkip) by file.Name into fileGroup
                where (fileGroup.Count() > 1)
                // narrows down results to only those that contain a pdf in it.
                select fileGroup;

            // catch (exception ex) { Console.WriteLine(ex); }
            //.ToList();
            // Substring(charsToSkip)

            // Pass the query to a method that will  
            // output one page at a time.
            string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string output = (path + "\\QueryResults " + ".csv");
            Console.WriteLine("The results have been sent to your desktop as a csv file called " + output);
            PageOutput<string, string>(queryDupNames);
    }
    

    // A Group key that can be passed to a separate method.  
    // Override Equals and GetHashCode to define equality for the key.  
    // Override ToString to provide a friendly name for Key.ToString()  
    class PortableKey
    {
        public string Name { get; set; }
        public DateTime LastWriteTime { get; set; }
        public long Length { get; set; }

        public override bool Equals(object obj)
        {
            PortableKey other = (PortableKey)obj;
            return other.LastWriteTime == this.LastWriteTime &&
                   other.Length == this.Length &&
                   other.Name == this.Name;
        }

        public override int GetHashCode()
        {
            string str = $"{this.LastWriteTime}{this.Length}{this.Name}";
            return str.GetHashCode();
        }
        public override string ToString()
        {
            return $"{this.Name} {this.Length} {this.LastWriteTime}";
        }
    }
    static void QueryDuplicates2()
    {
        // Change the root drive or folder if necessary.  
        string startFolder = @"c:\program files\Microsoft Visual Studio 9.0\Common7";

        // Make the lines shorter for the console display  
        int charsToSkip = startFolder.Length;

        // Take a snapshot of the file system.  
        System.IO.DirectoryInfo dir = new System.IO.DirectoryInfo(startFolder);
        IEnumerable<System.IO.FileInfo> fileList = dir.GetFiles("*.*", System.IO.SearchOption.AllDirectories);

        // Note the use of a compound key. Files that match  
        // all three properties belong to the same group.  
        // A named type is used to enable the query to be  
        // passed to another method. Anonymous types can also be used  
        // for composite keys but cannot be passed across method boundaries  
        //   
        var queryDupFiles =
            from file in fileList
            group file.FullName.Substring(charsToSkip) by
                new PortableKey { Name = file.Name, LastWriteTime = file.LastWriteTime, Length = file.Length } into fileGroup
            where fileGroup.Count() > 1
            select fileGroup;

        var list = queryDupFiles.ToList();

        int i = queryDupFiles.Count();

        PageOutput<PortableKey, string>(queryDupFiles);
    }

    // A generic method to page the output of the QueryDuplications methods  
    // Here the type of the group must be specified explicitly. "var" cannot  
    // be used in method signatures. This method does not display more than one  
    // group per page.  
    private static void PageOutput<K, V>(IEnumerable<System.Linq.IGrouping<K, V>> groupByExtList)
    {
        string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        string output = (path + "\\QueryResults " + ".csv");
        FileStream fs = new FileStream(output, FileMode.OpenOrCreate, FileAccess.Write);
        StreamWriter sWriter = new StreamWriter(fs);
        sWriter.BaseStream.Seek(0, SeekOrigin.End);
        // Flag to break out of paging loop.  
        bool goAgain = true;

        // "3" = 1 line for extension + 1 for "Press any key" + 1 for input cursor.  
        int numLines = Console.WindowHeight - 3;

        // Iterate through the outer collection of groups.  
        foreach (var filegroup in groupByExtList)
        {
            // Start a new extension at the top of a page.  
            int currentLine = 0;

            // Output only as many lines of the current group as will fit in the window.  
            do
            {
                
                sWriter.WriteLine("{0},", filegroup.Key.ToString() == String.Empty ? "[none]" : filegroup.Key.ToString());

                // Get 'numLines' number of items starting at number 'currentLine'.  
                var resultPage = filegroup.Skip(currentLine).Take(numLines);

                //Execute the resultPage query  
                foreach (var fileName in resultPage)
                {
                    sWriter.WriteLine(",{0}", fileName);
                }

                // Increment the line counter.  
                currentLine += numLines;

                // Give the user a chance to escape.  
                
               
            } while (currentLine < filegroup.Count());

            if (goAgain == false)
                break;
        }
    }
}
