// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.Organization.Presentation;

using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

public class OrganizationSignalRHub : Hub
{
    public async Task OnCheckHealth()
    {
        await this.Clients.All.SendAsync("CheckHealth");
    }
}