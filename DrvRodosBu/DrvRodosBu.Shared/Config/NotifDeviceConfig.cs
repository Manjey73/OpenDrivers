// Copyright (c) Rapid Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Scada.Comm.Devices;

namespace Scada.Comm.Drivers.DrvRodosBu.Config
{
    /// <summary>
    /// Represents a device configuration.
    /// <para>Представляет конфигурацию КП.</para>
    /// </summary>
    internal class NotifDeviceConfig : DeviceConfigBase
    {
        /// <summary>
        /// Gets or sets the request URI.
        /// </summary>
        public string Uri { get; set; }

        /// <summary>
        /// Gets or sets the request HTTP method.
        /// </summary>
        public RequestMethod Method { get; set; }

        /// <summary>
        /// Gets the request headers.
        /// </summary>
        public List<Header> Headers { get; private set; }

        /// <summary>
        /// Gets or sets the contents of the HTTP message.
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// Gets or sets the content type.
        /// </summary>
        public string ContentType { get; set; }

        ///// <summary>
        ///// Gets or sets the escaping method for content parameters.
        ///// </summary>
        //public EscapingMethod ContentEscaping { get; set; }

        /// <summary>
        /// Sets the default values.
        /// </summary>
        protected override void SetToDefault()
        {
            Uri = "";
            Method = RequestMethod.Get;
            Headers = new List<Header>();
            Content = "";
            ContentType = "";
            //ContentEscaping = EscapingMethod.None;
        }
    }
}
