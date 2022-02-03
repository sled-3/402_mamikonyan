using ObjectStorage.View.Alerts;
using ObjectStorage.ViewModel;
using System;
using System.Configuration;
using System.IO;
using System.Linq;

namespace ObjectStorage.Helpers
{
    public class ObjectStorageHelper
    {
        #region MenuTitle
        public static string StorageTable = "Изображения";
        public static string Gallary = "Галерея";
        public static string Objects = "Объекты";
        #endregion

        #region Icons
        public static string StorageTableIcon = "/Files/History.png";
        public static string GallaryIcon = "/Files/gallery.jpg";
        public static string ObjectsIcon = "/Files/objects.jpg";
        #endregion


        public static string ImageNA = "/Files/NA.png";

        public static string GetSaveFilePath()
        {
            string DirectoryPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string FolderName = "InventoryFiles";
            DirectoryPath += @"\" + FolderName;
            if (!Directory.Exists(DirectoryPath))
            {
                Directory.CreateDirectory(DirectoryPath);
            }
            return DirectoryPath;
        }


        public static readonly GrowlNotifiactions growlNotifications = new GrowlNotifiactions(); 

        public static void SimpleAlert(string _Title, string _Message)
        {
            growlNotifications.AddNotification(new Notification { Title = _Title, ImageUrl = "pack://application:,,,/Files/notification-icon.png", Message = _Message });

        }

        public static void SuccessAlert(string _Title, string _Message)
        {
            growlNotifications.AddNotification(new Notification { Title = _Title, ImageUrl = "pack://application:,,,/Files/Success.png", Message = _Message });

        }
        public static void ErrorAlert( string _Message,string _Title="Ошибка")
        {
            growlNotifications.AddNotification(new Notification { Title = _Title, ImageUrl = "pack://application:,,,/Files/Error.png", Message = _Message });

        }

    }


}


