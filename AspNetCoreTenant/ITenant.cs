using System;
using System.Collections.Generic;
using System.Text;

namespace Corpool.AspNetCoreTenant {
    /**
     * Interface specifying the field any Tenant entity implementation should have
     */
    public interface ITenant {
        public string Id { get; set; }
    }
}
