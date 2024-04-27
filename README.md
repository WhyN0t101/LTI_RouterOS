## SDN Controller for MikroTik Devices

This repository contains the source code and documentation for an SDN (Software-Defined Networking) controller developed in C# for managing MikroTik network devices running RouterOS. The controller allows for the configuration and supervision of various aspects of MikroTik devices, including interfaces, wireless networks, routes, DHCP servers, and DNS servers. Additionally, it implements a server/client VPN using WireGuard, with all configuration and management handled through the controller.

## Table of Contents

1. [Introduction](#introduction)
2. [Features](#features)
3. [Requirements](#requirements)
4. [Installation](#installation)
5. [Usage](#usage)
6. [License](#license)

## Introduction

The project aims to provide a centralized solution for managing MikroTik network infrastructure efficiently. By leveraging SDN principles, administrators can easily configure and monitor their devices through a user-friendly interface.

## Features

- **Interface Management:** Configure and control LAN and Bridge interfaces. Create, edit, and delete bridge interfaces and their ports.
- **Wireless Network Configuration:** Manage security profiles and wireless interfaces, including activation, deactivation, and editing.
- **Routing:** View, create, edit, and delete static routes.
- **IP Configuration:** Manage IP addresses, DHCP servers, and DNS settings.
- **WireGuard VPN:** Create, edit, and delete WireGuard VPN interfaces and peers.

## Requirements

- MikroTik devices running RouterOS with API REST enabled.
- .NET
- WireGuard installed on the MikroTik devices (if using the VPN feature)

## Installation

1. Clone this repository to your local machine.
2. Navigate to the `src` directory.
3. Build the project using the .NET Core SDK:


## Usage

1. Ensure your MikroTik devices are configured to allow API REST access.
2. Run the compiled application on your local machine or deploy it to a server.
3. Access the application through a web browser and log in with valid credentials.
4. Start managing your MikroTik devices and configuring the VPN through the intuitive interface.

## License

This project is licensed under the [MIT License].
