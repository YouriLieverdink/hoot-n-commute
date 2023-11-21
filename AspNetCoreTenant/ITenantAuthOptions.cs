using System;
using System.Collections.Generic;
using System.Text;

namespace Corpool.AspNetCoreTenant {
    /**
     * This interface specifies a configuration option that should be provided
     * to the tenant subsystem, allowing verification of valid claims.
     */
    public interface ITenantAuthOptions {
        public string Authority { get; set; }
    }
}
