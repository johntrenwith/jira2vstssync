# jira2vstssync
## Overview ##

JIRA2VSTSSync is a simple, .NET Core-based Console Application used to synchronise issues in [Atlassian's JIRA](https://www.atlassian.com/software/jira) with work items [Microsoft's Visual Studio Team Services (VSTS)](https://www.visualstudio.com/team-services/). The use case for this application is organisations wanting to track workflow, estimates, defects et cetera in JIRA whilst linking to VSTS for version control, build management, release management and documentation.

As of the current version, the flow of JIRA issues to VSTS work items is unidirectional - work items are created and updated based on information sourced from JIRA, but updates from VSTS are not propagated back to JIRA; JIRA is the sole master of work item information.

JIRA2VSTSSync only synchronises issues between one JIRA project and one VSTS team project. Bi-directional and multi-project synchronisation may be accommodated in future versions.

JIRA2VSTSSync uses the JIRA and VSTS REST APIs to query issues and work items and to create or update work items.  

## Configuration ##

In order to use JIRA2VSTSSync with an instance of VSTS, some initial configuration is required. 

Firstly, a field is required to store the value of the JIRA key in VSTS. If one of the [default process templates](https://docs.microsoft.com/en-us/vsts/work/work-items/guidance/choose-process#agile-cmmi-and-scrum) has been used as the process template for the target team project then an out-of-the-box field may be used for this purpose, but it's better to use a custom field to make its function explicit. In order to do this it's necessary to first create an [inherited template](https://docs.microsoft.com/en-us/vsts/work/customize/inheritance-process-model) based on one of the default process templates and then change the team project to use that template. Once that's done it's then possible to add a custom field that can be given an appropriate name, e.g. "JIRA Key". 

Secondly, a shared query must be created in VSTS that returns all work items with a JIRA key. This query is used by JIRA2VSTSSync to determine which issues have already been created as work items in the team project and what their status is.

All users assigned issues in JIRA must have corresponding user accounts in VSTS, otherwise it will not be possible to create the issues that they are assigned to.

To connect to VSTS via the REST API a personal access token (PAT) [must be created for the authenticating user](https://docs.microsoft.com/en-us/vsts/git/_shared/personal-access-tokens). The PAT should be authorised for the "Work items (full)" scope.

All JIRA and VSTS settings may be configured in the appsettings.json file in the project root directory.
