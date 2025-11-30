# TradeManagerPlus for ArchiSteamFarm

[![Steam donate](https://img.shields.io/badge/Steam-donate-000000.svg?logo=steam)](https://steamcommunity.com/tradeoffer/new/?partner=1211192445&token=T9Hiu3Oz)

[![BTC donate](https://img.shields.io/badge/BTC-donate-f7931a.svg?logo=bitcoin)](https://www.blockchain.com/explorer/addresses/btc/bc1qmkv939k2wqsej657cxj25ppwqdh65y2umnv3gg)
[![LTC donate](https://img.shields.io/badge/LTC-donate-a6a9aa.svg?logo=litecoin)](https://live.blockcypher.com/ltc/address/ltc1qjr49nr028mcajlt7prmmnqnjh0552qjj90zdq4)
[![XMR donate](https://img.shields.io/badge/XMR-donate-FF6600.svg?logo=monero)](82oJRDdiSWWbem3HiYx7ZdDdiPkYQAW4LaGNHpNcJ9DCendQ3XcxHNYQiRMtfghYtSMmARPGqKe2ddSrhtjviTraEyGwgZ2)

---

## DISCLAIMER: This Plugin is still far from finished. It does not work yet, is not useable and might break your ASF Installation if used in this State

## Contact

For business-related questions, you can reach me at:

- **Steam:** [steamcommunity.com/id/klb777](https://steamcommunity.com/id/klb777)
- **Discord:** killerboyyy777

---

## Description

TradeManagerPlus is a plugin for **[ArchiSteamFarm](https://github.com/JustArchiNET/ArchiSteamFarm)** that enhances its trading capabilities. It allows you to define custom rules to automatically accept or decline trade offers based on the items involved.

---

## Features

- **Custom Trade Rules**: Configure rules to automatically accept or decline trades based on item properties like AppID, type, and rarity.
- **Test Command**: A simple `TEST` command to verify that the plugin is loaded and working correctly.

---

## Installation

1. Download the latest release from the **[releases page](https://github.com/killerboyyy777/trademanagerplus/releases)**.
2. Extract the `trademanagerplus` folder into your ASF `plugins` directory.
3. Restart ASF.

---

## Configuration

You can configure the trade rules in your ASF bot config file (e.g., `MyBot.json`). The rules are defined within the `Plugins` configuration for `trademanagerplus`.

### Example Configuration

Here is an example of how to configure trade rules:

```json
{
  "Plugins": {
    "trademanagerplus": {
      "TradeRules": [
        {
          "Type": "Accept",
          "Conditions": {
            "TheirItems": {
              "AppIDs": [753],
              "Types": ["Trading Card"]
            },
            "MyItems": {
              "AppIDs": [753],
              "Types": ["Trading Card"]
            }
          },
          "ConditionLogic": "All",
          "Description": "Accept any 1:1 trade for Steam trading cards."
        },
        {
          "Type": "Decline",
          "Conditions": {
            "MyItems": {
              "Rarities": ["Rare"]
            }
          },
          "ConditionLogic": "Any",
          "Description": "Decline any trade where I'm giving away a rare item."
        }
      ]
    }
  }
}
```

### Rule Properties

- **`Type`**: The action to take if the conditions are met. Can be `Accept` or `Decline`.
- **`Conditions`**: A set of conditions that must be met for the rule to trigger.
  - **`TheirItems`**: Conditions for the items you are receiving.
  - **`MyItems`**: Conditions for the items you are giving.
- **`ConditionLogic`**: How the conditions are evaluated.
  - `All`: All specified conditions must be true.
  - `Any`: At least one of the specified conditions must be true.
- **`Description`**: An optional description of the rule.

### Item Property Conditions

Within `TheirItems` and `MyItems`, you can specify conditions based on the following item properties:

- **`AppIDs`**: A list of App IDs. The item's App ID must match one of the specified IDs.
- **`Types`**: A list of item types (e.g., "Trading Card", "Booster Pack", "Emoticon", "Profile Background").
- **`Rarities`**: A list of item rarities (e.g., "Common", "Uncommon", "Rare").

The plugin will process the rules in the order they are defined. The first rule that matches a trade offer will be executed, and no further rules will be checked for that offer. If no rules match, ASF's default trade handling logic will be used.

---

## Commands

- **`TEST`**: Responds with `Test command executed!` to confirm the plugin is working.
  - **Usage**: `!TEST <bot_name>`
