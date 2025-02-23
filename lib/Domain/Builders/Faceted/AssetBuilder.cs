﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Gotenberg.Sharp.API.Client.Domain.Requests;
using Gotenberg.Sharp.API.Client.Domain.Requests.Facets;
using Gotenberg.Sharp.API.Client.Extensions;

using JetBrains.Annotations;

namespace Gotenberg.Sharp.API.Client.Domain.Builders.Faceted
{
    public sealed class AssetBuilder : BaseFacetedBuilder<RequestBase>
    {
        public AssetBuilder(RequestBase request)
        {
            this.Request = request ?? throw new ArgumentNullException(nameof(request));
            Request.Assets ??= new AssetDictionary();
        }

        #region one asset

        [PublicAPI]
        public AssetBuilder AddItem(string name, ContentItem value)
        {
            // ReSharper disable once ComplexConditionExpression
            if (name.IsNotSet() || new FileInfo(name).Extension.IsNotSet() || name.LastIndexOf('/') >= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(name),
                    "Asset names must be relative file names with extensions");
            }

            this.Request.Assets.Add(name, value ?? throw new ArgumentNullException(nameof(value)));

            return this;
        }

        [PublicAPI]
        public AssetBuilder AddItem(string name, string value) =>
            AddItem(name, new ContentItem(value));

        [PublicAPI]
        public AssetBuilder AddItem(string name, byte[] value) =>
            AddItem(name, new ContentItem(value));

        [PublicAPI]
        public AssetBuilder AddItem(string name, Stream value) =>
            AddItem(name, new ContentItem(value));

        #endregion

        #region 'n' assets

        #region from dictionaries

        [PublicAPI]
        public AssetBuilder AddItems(Dictionary<string, ContentItem> items)
        {
            if (items == null) throw new ArgumentNullException(nameof(items));

            foreach (var item in items)
            {
                this.AddItem(item.Key, item.Value);
            }

            return this;
        }

        [PublicAPI]
        public AssetBuilder AddItems(Dictionary<string, string> assets) =>
            AddItems(assets?.ToDictionary(a => a.Key, a => new ContentItem(a.Value)));

        [PublicAPI]
        public AssetBuilder AddItems(Dictionary<string, byte[]> assets) =>
            AddItems(assets?.ToDictionary(a => a.Key, a => new ContentItem(a.Value)));

        [PublicAPI]
        public AssetBuilder AddItems(Dictionary<string, Stream> assets) =>
            AddItems(assets?.ToDictionary(a => a.Key, a => new ContentItem(a.Value)));

        #endregion

        #region from KVP enumerables

        [PublicAPI]
        public AssetBuilder AddItems(IEnumerable<KeyValuePair<string, ContentItem>> assets) =>
            AddItems(new Dictionary<string, ContentItem>(
                assets?.ToDictionary(a => a.Key, a => a.Value) ??
                throw new ArgumentNullException(nameof(assets))));

        [PublicAPI]
        public AssetBuilder AddItems(IEnumerable<KeyValuePair<string, string>> assets) =>
            AddItems(new Dictionary<string, ContentItem>(
                assets?.ToDictionary(a => a.Key, a => new ContentItem(a.Value)) ??
                throw new ArgumentNullException(nameof(assets))));

        [PublicAPI]
        public AssetBuilder AddItems(IEnumerable<KeyValuePair<string, byte[]>> assets) =>
            AddItems(assets?.ToDictionary(a => a.Key, a => new ContentItem(a.Value)) ??
                     throw new ArgumentNullException(nameof(assets)));

        [PublicAPI]
        public AssetBuilder AddItems(IEnumerable<KeyValuePair<string, Stream>> assets) =>
            AddItems(assets?.ToDictionary(s => s.Key, a => new ContentItem(a.Value)) ??
                     throw new ArgumentNullException(nameof(assets)));

        #endregion

        #endregion
    }
}