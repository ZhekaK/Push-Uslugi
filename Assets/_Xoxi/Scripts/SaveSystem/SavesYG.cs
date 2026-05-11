using System;
using System.IO;
using UnityEngine;

namespace YG
{
    [Serializable]
    public partial class SavesData
    {
        public Lang _lang;
        public Difficult difficult;
        public bool anim;
        public float animationSpeed;
        public bool sound;
        public bool timerTurn;
        public bool markLastFigure;
        public int[] gamesAll;
        public int[] gamesForCross;
        public int[] gamesForNulls;
        public int[] cellsFilled;
        public int[] wins;
        public int[] loses;
        public int gamesInLocalMode;
        public int crossWinsInLocalMode;
        public int nullsWinsInLocalMode;

        public static SavesData CreateDefault()
        {
            return new SavesData
            {
                _lang = Lang.ru,
                difficult = Difficult.easy,
                anim = true,
                animationSpeed = 0.15f,
                sound = true,
                timerTurn = true,
                markLastFigure = true,
                gamesAll = new int[3],
                gamesForCross = new int[3],
                gamesForNulls = new int[3],
                cellsFilled = new int[3],
                wins = new int[3],
                loses = new int[3]
            };
        }

        public void EnsureValid()
        {
            _lang = IsDefined(_lang) ? _lang : Lang.ru;
            difficult = IsDefined(difficult) ? difficult : Difficult.easy;
            animationSpeed = animationSpeed > 0f ? animationSpeed : 0.15f;

            gamesAll = EnsureArray(gamesAll, 3);
            gamesForCross = EnsureArray(gamesForCross, 3);
            gamesForNulls = EnsureArray(gamesForNulls, 3);
            cellsFilled = EnsureArray(cellsFilled, 3);
            wins = EnsureArray(wins, 3);
            loses = EnsureArray(loses, 3);
        }

        private static bool IsDefined<T>(T value) where T : struct
        {
            return Enum.IsDefined(typeof(T), value);
        }

        private static int[] EnsureArray(int[] values, int length)
        {
            if (values == null)
                return new int[length];

            if (values.Length == length)
                return values;

            int[] result = new int[length];
            Array.Copy(values, result, Math.Min(values.Length, length));
            return result;
        }
    }

    public class EnvirData
    {
        public string language = "ru";
    }

    public static class Saver
    {
        public static readonly EnvirData envir = new EnvirData();

        private static SavesData _saves;

        public static SavesData saves
        {
            get
            {
                if (_saves == null)
                    LoadProgress();

                return _saves;
            }
            set
            {
                _saves = value ?? SavesData.CreateDefault();
                _saves.EnsureValid();
            }
        }

        public static void LoadProgress()
        {
            saves = LocalBinarySave.Load();
        }

        public static void SaveProgress()
        {
            saves.EnsureValid();
            LocalBinarySave.Save(saves);
        }
    }

    internal static class LocalBinarySave
    {
        private const int SaveVersion = 1;
        private const string SaveFileName = "xoxi_save.dat";

        private static string SavePath
        {
            get { return Path.Combine(Application.persistentDataPath, SaveFileName); }
        }

        public static SavesData Load()
        {
            if (!File.Exists(SavePath))
                return SavesData.CreateDefault();

            try
            {
                using (FileStream stream = File.Open(SavePath, FileMode.Open, FileAccess.Read))
                using (BinaryReader reader = new BinaryReader(stream))
                {
                    int version = reader.ReadInt32();
                    if (version != SaveVersion)
                        return SavesData.CreateDefault();

                    SavesData data = new SavesData
                    {
                        _lang = (Lang)reader.ReadInt32(),
                        difficult = (Difficult)reader.ReadInt32(),
                        anim = reader.ReadBoolean(),
                        animationSpeed = reader.ReadSingle(),
                        sound = reader.ReadBoolean(),
                        timerTurn = reader.ReadBoolean(),
                        markLastFigure = reader.ReadBoolean(),
                        gamesAll = ReadIntArray(reader, 3),
                        gamesForCross = ReadIntArray(reader, 3),
                        gamesForNulls = ReadIntArray(reader, 3),
                        cellsFilled = ReadIntArray(reader, 3),
                        wins = ReadIntArray(reader, 3),
                        loses = ReadIntArray(reader, 3),
                        gamesInLocalMode = reader.ReadInt32(),
                        crossWinsInLocalMode = reader.ReadInt32(),
                        nullsWinsInLocalMode = reader.ReadInt32()
                    };

                    data.EnsureValid();
                    return data;
                }
            }
            catch (Exception exception)
            {
                Debug.LogWarning("Failed to load local save. Default values will be used. " + exception.Message);
                return SavesData.CreateDefault();
            }
        }

        public static void Save(SavesData data)
        {
            try
            {
                Directory.CreateDirectory(Application.persistentDataPath);

                using (FileStream stream = File.Open(SavePath, FileMode.Create, FileAccess.Write))
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    writer.Write(SaveVersion);
                    writer.Write((int)data._lang);
                    writer.Write((int)data.difficult);
                    writer.Write(data.anim);
                    writer.Write(data.animationSpeed);
                    writer.Write(data.sound);
                    writer.Write(data.timerTurn);
                    writer.Write(data.markLastFigure);
                    WriteIntArray(writer, data.gamesAll);
                    WriteIntArray(writer, data.gamesForCross);
                    WriteIntArray(writer, data.gamesForNulls);
                    WriteIntArray(writer, data.cellsFilled);
                    WriteIntArray(writer, data.wins);
                    WriteIntArray(writer, data.loses);
                    writer.Write(data.gamesInLocalMode);
                    writer.Write(data.crossWinsInLocalMode);
                    writer.Write(data.nullsWinsInLocalMode);
                }
            }
            catch (Exception exception)
            {
                Debug.LogWarning("Failed to save local data. " + exception.Message);
            }
        }

        private static int[] ReadIntArray(BinaryReader reader, int expectedLength)
        {
            int savedLength = reader.ReadInt32();
            int[] result = new int[expectedLength];

            for (int i = 0; i < savedLength; i++)
            {
                int value = reader.ReadInt32();
                if (i < expectedLength)
                    result[i] = value;
            }

            return result;
        }

        private static void WriteIntArray(BinaryWriter writer, int[] values)
        {
            values = values ?? new int[0];
            writer.Write(values.Length);

            for (int i = 0; i < values.Length; i++)
            {
                writer.Write(values[i]);
            }
        }
    }
}
