﻿using FreeSql.DataAnnotations;
using Newtonsoft.Json;
using PRM.Core.DB.IDB;
using PRM.Core.Protocol;

namespace PRM.Core.DB.freesql
{
    [Table(Name = "Server")]
    public class DbServer: IDbServer
    {
        [Column(IsIdentity = true, IsPrimary = true)]
        public int Id { get; set; }
        [JsonProperty, Column(DbType = "VARCHAR")]
        public string Protocol { get; set; } = "";
        [JsonProperty, Column(DbType = "VARCHAR")]
        public string ClassVersion { get; set; } = "";
        [JsonProperty, Column(DbType = "VARCHAR")]
        public string JsonConfigString { get; set; } = "";
        /// <summary>
        /// identify when it updated
        /// </summary>
        [JsonProperty, Column(DbType = "VARCHAR")]
        public string UpdatedToken { get; set; } = "";

        public ProtocolServerBase ToProtocolServerBase()
        {
            return ItemCreateHelper.CreateFromDbOrm(this);
        }

        public int GetId()
        {
            return Id;
        }

        public string GetProtocol()
        {
            return Protocol;
        }

        public string GetClassVersion()
        {
            return ClassVersion;
        }

        public string GetJson()
        {
            return JsonConfigString;
        }

        public string GetUpdatedMark()
        {
            return UpdatedToken;
        }
    }

    static class DbServerHelperStatic
    {
        public static DbServer ToDbServer(this ProtocolServerBase s)
        {
            var ret = new DbServer()
            {
                Id = s.Id,
                ClassVersion = s.ClassVersion,
                JsonConfigString = s.ToJsonString(),
                Protocol = s.Protocol,
            };
            return ret;
        }
    }
}