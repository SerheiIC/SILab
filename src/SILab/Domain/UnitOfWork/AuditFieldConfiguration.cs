﻿namespace SILab.Domain.UnitOfWork
{
    public class AuditFieldConfiguration
    {
        public string FieldName { get; set; }

        public bool IsSavingEnabled { get; }

        public AuditFieldConfiguration(string fieldName, bool isSavingEnabled)
        {
            FieldName = fieldName;
            IsSavingEnabled = isSavingEnabled;
        }
    }
}
