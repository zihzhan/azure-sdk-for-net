﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using System;
using System.Threading.Tasks;

using Azure.ResourceManager.Resources;
using Azure.ResourceManager.EventHubs.Models;
using Azure.ResourceManager.EventHubs.Tests;
using Azure.ResourceManager.Network;
using Azure.ResourceManager.Network.Models;
using NUnit.Framework;

namespace Azure.Management.EventHub.Tests
{
    public partial class ScenarioTests : EventHubsManagementClientBase
    {
        [Test]
        public async Task NetworkRuleSetCreateGetUpdateDelete()
        {
            var location = await GetLocation();
            var resourceGroupName = Recording.GenerateAssetName(Helper.ResourceGroupPrefix);
            ResourceGroup resourceGroup = await ArmClient.DefaultSubscription.GetResourceGroups().CreateOrUpdate(resourceGroupName, new ResourceGroupData(location)).WaitForCompletionAsync();

            // Prepare VNet
            var vnetName = Recording.GenerateAssetName("sdktestvnet");
            var parameters = new VirtualNetworkData
            {
                AddressSpace = new AddressSpace { AddressPrefixes = { "10.0.0.0/16" } },
                Subnets = {
                    new SubnetData
                    {
                        Name = "default1",
                        AddressPrefix = "10.0.0.0/24",
                        ServiceEndpoints = { new ServiceEndpointPropertiesFormat { Service = "Microsoft.EventHub" } }
                    },
                    new SubnetData
                    {
                        Name = "default2",
                        AddressPrefix = "10.0.1.0/24",
                        ServiceEndpoints = { new ServiceEndpointPropertiesFormat { Service = "Microsoft.EventHub" } }
                    },
                    new SubnetData
                    {
                        Name = "default3",
                        AddressPrefix = "10.0.2.0/24",
                        ServiceEndpoints = { new ServiceEndpointPropertiesFormat { Service = "Microsoft.EventHub" } }
                    }
                },
                Location = "eastus2"
            };
            await WaitForCompletionAsync(await resourceGroup.GetVirtualNetworks().CreateOrUpdateAsync(vnetName, parameters));

            // Create a namespace
            var namespaceName = Recording.GenerateAssetName(Helper.NamespacePrefix);
            var createNamespaceResponse = await NamespacesOperations.StartCreateOrUpdateAsync(resourceGroupName, namespaceName,
                new EHNamespace()
                {
                    Location = location,
                    Tags =
                        {
                            {"tag1", "value1"},
                            {"tag2", "value2"}
                        }
                }
                );
            var np = (await WaitForCompletionAsync(createNamespaceResponse)).Value;
            Assert.NotNull(createNamespaceResponse);
            Assert.AreEqual(np.Name, namespaceName);
            DelayInTest(5);
            //get the created namespace
            var getNamespaceResponse = await NamespacesOperations.GetAsync(resourceGroupName, namespaceName);
            if (string.Compare(getNamespaceResponse.Value.ProvisioningState, "Succeeded", true) != 0)
                DelayInTest(5);
            getNamespaceResponse = await NamespacesOperations.GetAsync(resourceGroupName, namespaceName);
            Assert.NotNull(getNamespaceResponse);
            Assert.AreEqual("Succeeded", getNamespaceResponse.Value.ProvisioningState, StringComparer.CurrentCultureIgnoreCase.ToString());
            Assert.AreEqual(location, getNamespaceResponse.Value.Location, StringComparer.CurrentCultureIgnoreCase.ToString());

            var netWorkRuleSet = await NamespacesOperations.CreateOrUpdateNetworkRuleSetAsync(resourceGroupName, namespaceName,
                new NetworkRuleSet()
                {
                    DefaultAction = DefaultAction.Deny,
                    VirtualNetworkRules =
                    {
                        new NWRuleSetVirtualNetworkRules() { Subnet = new ResourceManager.EventHubs.Models.Subnet("/subscriptions/" + SubscriptionId + "/resourcegroups/"+ resourceGroupName + "/providers/Microsoft.Network/virtualNetworks/"+ vnetName + "/subnets/default1") },
                        new NWRuleSetVirtualNetworkRules() { Subnet = new ResourceManager.EventHubs.Models.Subnet("/subscriptions/" + SubscriptionId + "/resourcegroups/"+ resourceGroupName + "/providers/Microsoft.Network/virtualNetworks/"+ vnetName + "/subnets/default2") },
                        new NWRuleSetVirtualNetworkRules() { Subnet = new ResourceManager.EventHubs.Models.Subnet("/subscriptions/" + SubscriptionId + "/resourcegroups/"+ resourceGroupName + "/providers/Microsoft.Network/virtualNetworks/"+ vnetName + "/subnets/default3") }
                    },
                    IpRules =
                    {
                        new NWRuleSetIpRules() { IpMask = "1.1.1.1", Action = "Allow" },
                        new NWRuleSetIpRules() { IpMask = "1.1.1.2", Action = "Allow" },
                        new NWRuleSetIpRules() { IpMask = "1.1.1.3", Action = "Allow" },
                        new NWRuleSetIpRules() { IpMask = "1.1.1.4", Action = "Allow" },
                        new NWRuleSetIpRules() { IpMask = "1.1.1.5", Action = "Allow" }
                    }
                });
            var getNetworkRuleSet = await NamespacesOperations.GetNetworkRuleSetAsync(resourceGroupName, namespaceName);
            var netWorkRuleSet1 = await NamespacesOperations.CreateOrUpdateNetworkRuleSetAsync(resourceGroupName, namespaceName, new NetworkRuleSet() { DefaultAction = "Allow" });
            var getNetworkRuleSet1 = await NamespacesOperations.GetNetworkRuleSetAsync(resourceGroupName, namespaceName);
            DelayInTest(60);
            //Delete namespace
            await WaitForCompletionAsync(await NamespacesOperations.StartDeleteAsync(resourceGroupName, namespaceName));
        }
    }
}
