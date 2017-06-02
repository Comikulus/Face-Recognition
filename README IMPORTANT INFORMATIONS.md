# Face-Recognition
Authentification via Face Recognition using Microsoft Cognitive Services and Azure Database. The application is in Hungarian
If you want to use this app, all you need is Microsoft Azure Account with Azure Pass. You need to create an AzureStorageAccount (ConnectionString in App.config), use Cognitive Services ( APIKEY + faceListId) and SqlDatabase (Connection String)
There are some packages that are required to run this app:

using System.Data.SqlClient;
using AForge;
using AForge.Video;
using AForge.Video.DirectShow;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Web;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Drawing.Imaging;
using System.Text.RegularExpressions;
 
 
 The application is using a Database with two tables:
 
 CheckInUsers (This is where we store infomation about the photographers):
 CREATE TABLE [dbo].[CheckInUsers] (
    [UserID]        INT        IDENTITY (1, 1) NOT NULL,
    [Username]      NCHAR (15) NOT NULL,
    [Password]      NCHAR (15) NOT NULL,
    [NameContainer] NCHAR (15) NOT NULL,
    [ImgNum]        INT        DEFAULT ((0)) NOT NULL,
    PRIMARY KEY CLUSTERED ([UserID] ASC)
);
 
 
 
 
 
 
 Participants(This is where we store informations from the registered participants):
 CREATE TABLE [dbo].[Participants] (
    [ParticipantID]   INT         IDENTITY (1, 1) NOT NULL,
    [Surname]         NCHAR (30)  NOT NULL,
    [Forename]        NCHAR (40)  NOT NULL,
    [Birth]           DATETIME    NOT NULL,
    [Email]           NCHAR (30)  NOT NULL,
    [Company]         NCHAR (30)  NOT NULL,
    [Field]           NCHAR (30)  NOT NULL,
    [persistedFaceId] NCHAR (150) NOT NULL,
    [ImgUrl]          NCHAR (300) NOT NULL,
    [CheckedIn]       BIT         DEFAULT ((0)) NOT NULL,
    PRIMARY KEY CLUSTERED ([ParticipantID] ASC)
);

In order to store the registered faces I'm using a FaceList and the method used to identify the person is FindSimilar which returns the persistedFaceId of the person that we are looking for.


If you have any questions don't hesitate to ask me :)
 

