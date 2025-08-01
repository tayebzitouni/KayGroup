using DataAccessLayer.Models;
using DataAccessLayer;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Speech.Recognition;

public static class UserSettingsManager
{
    public static UserSettings Load(AppDbContext context)
    {
        var settings = context.UserSettings.FirstOrDefault();
        if (settings == null || settings.Id == 0)
        {
            settings = new UserSettings();
            UserSettingsManager.Save(context, settings);
        }
        return settings;
    }

    public static void Save(AppDbContext context, UserSettings settings)
    {
        var current = context.UserSettings.FirstOrDefault();

        if (current != null)
        {
            //if (current.LastModified != settings.LastModified)
            //    throw new Exception("الإعدادات تم تعديلها من طرف آخر.");
           
            current.TvaRate = settings.TvaRate;
            current.IsRate = settings.IsRate;
            current.RetenueRate = settings.RetenueRate;
            current.Annefiscal = settings.Annefiscal;
            current.Name = settings.Name;
            current.Devis = settings.Devis;
            current.LastModified = DateTime.Now;
        }
        else
        {
            settings.LastModified = DateTime.Now;
            context.UserSettings.Add(settings);
        }

        context.SaveChanges();
    }
}

