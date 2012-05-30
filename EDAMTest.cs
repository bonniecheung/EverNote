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
using SampleApp.Database.DataManager;
using SampleApp.Domain.Objects;

public class EDAMTest {
    public static void Main(string[] args) {

        // Data source name string
        String connectionString = "DSN=EverNoteDB;Uid=postgres;Pwd=tindrbonnie;";

        // Developer token for Evernote API
        String authToken = "S=s1:U=2261f:E=13eccfa5d71:C=13775493171:P=1cd:A=en-devtoken:H=fe7ee8dac619c7d380a0f9f3731a8698";

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

        // Lists all of the notebooks in the user's account with a notebook count       
        List<Notebook> notebooks = noteStore.listNotebooks(authToken);
        Console.WriteLine("Found " + notebooks.Count + " notebooks:");
        foreach (Notebook notebook in notebooks) {
            Console.WriteLine("  * " + notebook.Name);
        }

        //The offset represents the index of the first note that is to be returned
        int offset = 0;

        //maxNotes represents the number of note that is to be returned
        int maxNotes = 15;
        
        //NoteFilter is a struct that contains a list of criteria you can search notes for.
        //In this app, NoteFilter.Words will be used to return notes containing a certain string.
        NoteFilter filter = new NoteFilter();

        Console.WriteLine();
        Console.WriteLine("What word do you want to search through the notes for?");
        Console.WriteLine("Enter word: ");
        filter.Words = Console.ReadLine();
        //filter.Words = "business";
        
        //Creates a new NoteList struct, which returns a list of individual notes out of a larger set of them
        NoteList noteList = new NoteList();

        //This finds all notes containing the word that was given by the user
        noteList = noteStore.findNotes(authToken, filter, offset, maxNotes);

        //This stores the Guids of the notes containing the word
        ArrayList guidList = new ArrayList();

        // For each note in the list of notes...
        foreach (Note fetchedNote in noteList.Notes)
        {
            //Console.WriteLine("  * " + fetchedNote.Guid);

            // Add guid to list
            guidList.Add(fetchedNote.Guid);
        }

        // For each guid in the list...
        foreach (String Guid in guidList)
        {
            // Add block of plain text of the note to a list
            String text = noteStore.getNoteContent(authToken, Guid);
            //contentsList.Add(text);

            string[] split = Regex.Split(text, "\\n");
            
            Console.WriteLine();

            int MAX = split.Length;

            String line1;
            String line2;
            String line3;

            // This for-loop cycles through each line in the array of lines and tests if the line contains the word.
            // If so, it sets the note Guid as well as the surrounding lines as properties to a new Entry object.

            for (int n = 0; n < MAX; n++)
            {
                Boolean b = split[n].Contains(filter.Words);

                if (b == true)
                {
                    line2 = split[n];
                    line1 = split[n - 1];
                    line3 = split[n + 1];

                    Entry entry = new Entry();
                    entry.setGuid(Guid);
                    entry.setLine1(line1);
                    entry.setLine2(line2);
                    entry.setLine3(line3);

                    Console.WriteLine("Note GUID: " + entry.getGuid());
                    Console.WriteLine();
                    entry.displayLines();
                    entry.InsertRow(connectionString);
                }
            }
        }

        Console.WriteLine();
        Console.WriteLine("These lines have been saved to a Postgresql database under the table Notes.");
        Console.WriteLine();

        FNHSessionManager<NoteEntry> sessionManager = new FNHSessionManager<NoteEntry>(connectionString, FNHSessionManager<NoteEntry>.DatabaseType.Postgres);
        FNHRepository<NoteEntry> repository = new FNHRepository<NoteEntry>(sessionManager);

        NoteEntry noteEntry = repository.RetrieveById(1);

        Console.WriteLine("This is entry 1 retrieved from the database...");
        Console.WriteLine("GUID: " + noteEntry.Guid);
        Console.WriteLine("Line 1: " + noteEntry.Line1);
        Console.WriteLine("Line 2: " + noteEntry.Line2);
        Console.WriteLine("Line 3: " + noteEntry.Line3);

        Console.WriteLine("Click ENTER to continue...");
        Console.ReadLine();

    }
}
