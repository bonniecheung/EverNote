/*
  A simple Evernote application that authenticates with the
  Evernote web service, selects all notes containing the word "business"
  and saves the three surrounding lines to a database.
  
*/

using System;
using System.Drawing;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using Thrift;
using Thrift.Protocol;
using Thrift.Transport;
using Evernote.EDAM.Type;
using Evernote.EDAM.UserStore;
using Evernote.EDAM.NoteStore;
using Evernote.EDAM.Error;
using SampleApp.Properties;
using SampleApp;

public class EDAMTest {
    public static void Main(string[] args) {

        String connectionString = "DSN=EverNoteDB;Uid=postgres;Pwd=tindrbonnie;";

        String authToken = "S=s1:U=2261f:E=13eccfa5d71:C=13775493171:P=1cd:A=en-devtoken:H=fe7ee8dac619c7d380a0f9f3731a8698";

        if (authToken == "your developer token") {
          Console.WriteLine("Please fill in your developer token");
          Console.WriteLine("To get a developer token, visit https://sandbox.evernote.com/api/DeveloperToken.action");
          return;
        }

        String evernoteHost = "sandbox.evernote.com";
                
        Uri userStoreUrl = new Uri("https://" + evernoteHost + "/edam/user");
        TTransport userStoreTransport = new THttpClient(userStoreUrl);
        TProtocol userStoreProtocol = new TBinaryProtocol(userStoreTransport);
        UserStore.Client userStore = new UserStore.Client(userStoreProtocol);
        
        bool versionOK =
            userStore.checkVersion("Evernote EDAMTest (C#)",
        	   Evernote.EDAM.UserStore.Constants.EDAM_VERSION_MAJOR,
        	   Evernote.EDAM.UserStore.Constants.EDAM_VERSION_MINOR);
        Console.WriteLine("Is my Evernote API version up to date? " + versionOK);
        if (!versionOK) {
            return;
        }

        // Get the URL used to interact with the contents of the user's account
        // When your application authenticates using OAuth, the NoteStore URL will
        // be returned along with the auth token in the final OAuth request.
        // In that case, you don't need to make this call.
        String noteStoreUrl = userStore.getNoteStoreUrl(authToken);

        TTransport noteStoreTransport = new THttpClient(new Uri(noteStoreUrl));
        TProtocol noteStoreProtocol = new TBinaryProtocol(noteStoreTransport);
        NoteStore.Client noteStore = new NoteStore.Client(noteStoreProtocol);

        // List all of the notebooks in the user's account        
        List<Notebook> notebooks = noteStore.listNotebooks(authToken);
        Console.WriteLine("Found " + notebooks.Count + " notebooks:");
        foreach (Notebook notebook in notebooks) {
            Console.WriteLine("  * " + notebook.Name);
        }

        int offset = 0;
        int maxNotes = 15;
        NoteFilter filter = new NoteFilter();
        filter.Words = "business";
        NoteList noteList = new NoteList();

        //This finds all notes containing the word "business"
        noteList = noteStore.findNotes(authToken, filter, offset, maxNotes);

        Console.WriteLine();
        Console.WriteLine("The results of the query..." + noteList);
        Console.WriteLine();

        //This stores the Guids of the notes containing the word "business"
        ArrayList guidList = new ArrayList();

        //This stores the entire note contents of all notes containing the word "business"
        ArrayList contentsList = new ArrayList();

        //This stores all lines surrounding the line containing the word "business"
        ArrayList linesContaining = new ArrayList();

        // For each note in the list of notes...
        foreach (Note fetchedNote in noteList.Notes)
        {
            Console.WriteLine("  * " + fetchedNote.Guid);

            // Add guid to list
            guidList.Add(fetchedNote.Guid);
        }

        // For each guid in the list...
        foreach (String Guid in guidList)
        {
            // Add block of plain text of the note to a list
            String text = noteStore.getNoteContent(authToken, Guid);
            contentsList.Add(text);

            string[] split = Regex.Split(text, "\\n");
            
            Console.WriteLine();

            int max = split.Length;

            String line1;
            String line2;
            String line3;

            // This for-loop cycles through each line in the array of lines and tests if the line contains the word "business".
            // If so, it adds the surrounding lines to the linesContaining arrayList.

            for (int n = 0; n < max; n++)
            {
                Boolean b = split[n].Contains(filter.Words);

                if (b == true)
                {
                    line2 = split[n];
                    line1 = split[n - 1];
                    line3 = split[n + 1];

                    linesContaining.Add(line1);
                    linesContaining.Add(line2);
                    linesContaining.Add(line3);

                    Entry entry = new Entry();
                    entry.setGuid(Guid);
                    entry.setLine1(line1);
                    entry.setLine2(line2);
                    entry.setLine3(line3);

                    Console.WriteLine("This is the contents of the Entry object...");
                    entry.displayGuid();
                    entry.displayLines();
                    
                    String insertEntry = "INSERT INTO notes (guid, line1, line2, line3) " +
         "VALUES ('" + entry.getGuid() + "','" + entry.getLine1() + "','" + entry.getLine2() + "', '" + entry.getLine3() + "')";
                     
                    Console.WriteLine(insertEntry);
                    entry.InsertRow(connectionString, insertEntry);
                }
            }
        }

        Console.WriteLine();

        Console.WriteLine("Click ENTER to continue...");
        Console.ReadLine();

    }
}
