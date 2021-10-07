# dfc-api-lmi-delta-report

## Introduction

This project provides a LMI Delta Report function app app whihc is used to determine differences in consecutive LMI imports. The delta reports are stored in a Cosmos database for later use by the LMI Delta Report app.

## Getting started

This is a self-contained Visual Studio 2019 solution containing a number of projects (function application, with an associated unit test project).

### Installing

Clone the project and open the solution in Visual Studio 2019.

## List of dependencies

|Item|Purpose|
|----|-------|
|Azure Cosmos DB|Document storage |
|DFC.Compui.Cosmos|Cosmos DB interface|
|DFC.Compui.Subscriptions|Subscriptions API client|
|DFC.Swagger.Standard|DFC Swagger generator|

## Local Config Files

Once you have cloned the public repo you need to create app settings files from the configuration files listed below.

|Location|Filename|Rename to|
|--------|--------|---------|
|DFC.Api.Lmi.Delta.Report|local.settings-template.json|local.settings.json|

## Configuring to run locally

The project contains *local.settings-template.json* files which contains appsettings for the web app. To use these files, copy them to *local.settings.json* within each project and edit and replace the configuration item values with values suitable for your environment.

By default, the local.settings include local Azure Cosmos Emulator configurations using the well known Cosmos configuration values for Delta report headers data and Delta report details data storage (in separate collections). These may be changed to suit your environment if you are not using the Azure Cosmos Emulator.

## Running locally

To run this product locally, you will need to configure the list of dependencies, once configured and the configuration files updated, it should be F5 to run and debug locally.

To run the project, start the function application. Once running, use a tool like Postman to submit requests.

## Deployments

This LMI Delta Report API function app will be deployed as an individual deployment as part of the LMI quick win project.

## Built with

* Microsoft Visual Studio 2019
* .Net Core 3.1

## References

Please refer to [https://github.com/SkillsFundingAgency/dfc-digital](https://github.com/SkillsFundingAgency/dfc-digital) for additional instructions on configuring individual components like Cosmos.
