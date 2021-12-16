---
title: Command Line Tasks
description: The home of command line task documentation
linkText: Command Line Tasks
---

# Command Line Tasks

## Overview

There are various different tasks that you can achieve with Ensconce via the command line arguments.

This section will detail each type of use case and explain what command line arguments you should use to call Ensconce.

## Raw Command Line Help

```powershell
  -i, --stdin                Read template string from StdIn
  -h, --help                 Show this message and exit
      --fixedPath=VALUE      Override path to structure.xml relative to
                               executable (default="Env:\FixedPath")
  -s, --substitutionPath=VALUE
                             Path to substition file, relative to executable. (
                               default="substitutions.xml")
  -d, --databaseName=VALUE   The name of the database to be deployed, assumes
                               that the process is running on the destination
                               server. Requires the deployFrom option. Can
                               optionally provide the databaseRepository option.
      --connectionString=VALUE
                             The connection string for the database to be
                               deployed, Requires the deployFrom option. Can
                               optionally provide the databaseRepository option.
      --databaseRepository=VALUE
                             The entry to be made in the repository field in
                               the RoundhousE version table. If not provided
                               defaults to an empty string. NOTE! Ignored if
                               databaseName is not provided.
      --databaseCommandTimeout=VALUE
                             Database Command Timeout period in seconds. If not
                               provided defaults to a set value or 30s if not
                               set.
  -t, --deployTo=VALUE       Path to deploy to. Required for the copyTo &
                               replace option, multiple values can be specified.
  -f, --deployFrom=VALUE     Path to deploy from. Required for the copyTo &
                               replace and databaseName options
  -c, --copyTo               Add the contents of the deployFrom directory to
                               the deployTo directories, replacing files with
                               the same name
  -r, --replace              Replace the current contents of the deployTo
                               directories
  -u, --updateConfig         Update config
      --ofc, --outputFailureContext
                             Output Failure Context
  -q, --quiet                Turn off logging output (default=False, but always
                               True if -i set)
      --treatAsTemplateFilter=VALUE
                             File filter for files in the deploy from directory
                               to treat as templates. These will be updated
                               after config and before deployment.
      --warnOnOneTimeScriptChanges=VALUE
                             If one-time-scripts have had changes, only treat
                               them as warnings, not as errors. Defaults to
                               False.
      --withTransaction=VALUE
                             Execute RoundhousE in transactional mode. Defaults
                               to True.
      --roundhouseOutputPath=VALUE
                             Specify a directory for RoundhousE to store SQL
                               files. Defaults to E:\RH\
      --dropDatabase         Drop database, useful if you need to test
                               installations on a fresh database or need
                               control of databases for performance/load tests.
      --dropDatabaseConfirm  Drop database Confirmation, used to confirm that
                               database is to be dropped (for safety)
      --dr, --deployReports  Deploy Reporting service reports. See
                               reportVariable help for example usage.
      --drr, --deployReportingRole
                             Deploy Reporting service role for User. See
                               reportVariable help for example usage.
      --rsv, --reportVariable=VALUE
                             Deploy either reports (.rdl files) or add a role
                               to a local/domain user.

                               Both options requires Reporting Service
                               Variables (--rsv). multiple values can (and must)
                                be specified.

                               Example Usage:

                               # Deploy Reports
                                --dr
                                --rsv reportingServicesUrl=http://
                               reportingservices.yourserver.com:80/ReportServer_
                               INSTANCE/ReportService2010.asmx
                                --rsv networkDomain=DOMAIN
                                --rsv networkLogin=USER
                                --rsv networkPassword=PASSWORD

                                --rsv parentFolder=PARENT_FOLDER
                                --rsv subFolder=SUB_FOLDER
                                --rsv dataSourceName=DATA-SOURCE-NAME
                                --rsv dataSourceConnectionString=Data Source=(
                               local);Initial Catalog=DATABASE01
                                --rsv dataSourceUserName=DBUSER1
                                --rsv dataSourcePassword=DBPASSWORD1
                                --rsv reportSourceFolder=C:\TEMP\PATH_TO_
                               REPORTS\SUB_FOLDER

                               # Deploy Reporting Role for User
                                --drr
                                --rsv reportingServicesUrl=http://
                               reportingservices.yourserver.com:80/ReportServer_
                               INSTANCE/ReportService2010.asmx
                                --rsv networkDomain=DOMAIN
                                --rsv networkLogin=USER
                                --rsv networkPassword=PASSWORD

                                --rsv itemPath=/PARENT_FOLDER
                                --rsv reportingUserToAddRoleFor=DOMAIN\USER
                                --rsv reportingRoleToAdd="Content Manager"
      --dictionaryPostUrl=VALUE
                             Specify a url to post the tag directory to (as
                               JSON)
      --dictionarySavePath=VALUE
                             Specify a file to save the tag directory to (as
                               JSON)
      --backupSource=VALUE   Specify the source directory of the backup,
                               multiple values can be specified.  Required for
                               the backup option
      --backupDestination=VALUE
                             Specify the destination file for the backup.
                               Required for the backup option
      --bo, --backupOverwrite
                             Specify if the backup should overwrite an existing
                               file
```
