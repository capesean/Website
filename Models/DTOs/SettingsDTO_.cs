using System;
using System.Collections.Generic;

namespace WEB.Models
{
    public partial class DbSettingsDTO
    {
        public string TestField { get; set; }

        public DbSettingsDTO()
        {
        }
    }

    public static partial class ModelFactory
    {
        public static DbSettingsDTO Create(DbSettings settings)
        {
            if (settings == null) return null;

            var settingsDTO = new DbSettingsDTO();

            settingsDTO.TestField = settings.TestField;

            return settingsDTO;
        }

        public static void Hydrate(DbSettings settings, DbSettingsDTO settingsDTO)
        {
            settings.TestField = settingsDTO.TestField;
            //settings.VarBinary = Convert.FromBase64String(settingsDTO.VarBinaryString);
        }
    }
}
