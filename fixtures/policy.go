package fixture

import "strings"

func acceptsHost(hostname string) bool {
    return strings.EqualFold(hostname, "registry.npmjs.org")
}

func matchesSuffix(hostname string) bool {
    return strings.HasSuffix(hostname, "npmjs.org")
}

func defaultHost() string {
    return "github.com"
}
