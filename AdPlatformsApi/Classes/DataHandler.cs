﻿using AdPlatforms.Controllers;
using System.Text.RegularExpressions;

namespace AdPlatforms.Classes
{
    public class DataHandler
    {
        // Регулярное выражение для поиска локаций
        private static readonly Regex locationRegex = new Regex(@"\/[\w\/]+", RegexOptions.Compiled);
        // Словарь, ключи - локации, а значения - хэш-таблица платформ для этой локации
        private static Dictionary<string, HashSet<string>> platsLocationsDict = new();

        /// <summary>
        /// Метод получения платформ по локации
        /// </summary>
        /// <param name="location"></param>
        /// <returns></returns>
        public IEnumerable<string> GetPlatforms(string location)
        {
            return platsLocationsDict.TryGetValue(location, out var result) ? result : Array.Empty<string>();
        }

        /// <summary>
        /// Парсим текстовый файл с перечнем платформ и локаций
        /// </summary>
        /// <param name="data"></param>
        /// <exception cref="FormatException">Ошибка при обработке текстового файла</exception>
        public int ParseFile(string data)
        {
            var tempPlatsLocationsDict = new Dictionary<string, HashSet<string>>();

            // Парсим строки
            var lines = data.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            for (int i = 0; i < lines.Length; i++)
            {
                // Делим строку на две части
                var parts = lines[i].Split(":", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

                //Проверка, что строка поделилась на две части, иначе выкидываем исключение
                if (parts.Length != 2)
                    throw new FormatException($"Ошибка при обработке текстового файла - некорректные данные в строке №{i}: {lines[i]}");

                // Название платформы
                var platName = parts[0];
                // Список локаций платформы
                var locStr = parts[1];

                var locationMatches = locationRegex.Matches(locStr);
                if (locationMatches.Count > 0)
                {
                    // Для каждой локации в найденных метчах 
                    foreach (Match loc in locationMatches)
                        AddDataToDict(platName, loc.Value, tempPlatsLocationsDict);
                }
                else
                    throw new FormatException($"Ошибка при обработке текстового файла - строка №{i} не содержит локаций");
            }

            platsLocationsDict = tempPlatsLocationsDict;
            return platsLocationsDict.Keys.Count;
        }

        private void AddDataToDict(string platName, string loc, Dictionary<string, HashSet<string>> dict)
        {
            var tempLoc = loc;
            // Цикл - пока tempLoc не будет пустой строкой
            while (!string.IsNullOrWhiteSpace(tempLoc))
            {
                // Если в словаре нет записи с таким ключом - добавляем запись в словарь
                if (!dict.ContainsKey(tempLoc))
                    dict.Add(tempLoc, new HashSet<string>());

                // Добавляем по ключу tempLoc новую платформу в хэш-таблицу
                dict[tempLoc].Add(platName);

                // Берем локацию на один уровень выше и сохраняем в tempLoc
                int lastSlash = tempLoc.LastIndexOf('/');
                tempLoc = lastSlash > 0 ? tempLoc[..lastSlash] : string.Empty;
            }
        }
    }
}
