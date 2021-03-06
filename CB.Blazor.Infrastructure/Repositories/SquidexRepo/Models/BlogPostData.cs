﻿using Newtonsoft.Json;
using Squidex.ClientLibrary;
using System;
using System.Collections.Generic;

namespace CB.Blazor.Infrastructure.Repositories.SquidexRepo.Models
{
    public class BlogPostData
    {
        [JsonConverter(typeof(InvariantConverter))]
        public string Title { get; set; }

        [JsonConverter(typeof(InvariantConverter))]
        public string Body { get; set; }

        [JsonConverter(typeof(InvariantConverter))]
        public List<string> Image { get; set; }

        [JsonConverter(typeof(InvariantConverter))]
        public DateTime PublishDate { get; set; }

        [JsonConverter(typeof(InvariantConverter))]
        public List<string> Skills { get; set; }

        public BlogPostData()
        {
            Skills = new List<string>();
            Image = new List<string>();
        }
    }
}
