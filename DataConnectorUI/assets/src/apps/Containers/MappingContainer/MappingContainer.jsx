import React, { useState, useEffect } from 'react';
import ManageRule from '../../../components/ManageRule/ManageRule';
import MappingsBuilder from '../../../components/MappingsBuilder/MappingsBuilder';
import {
  Button,
  Grid,
  Snackbar,
  IconButton,
  Paper,
  Breadcrumbs,
  Link,
  Typography,
} from '@material-ui/core';
import NewMapping from '../../../components/NewMapping/NewMapping';
import gql from 'graphql-tag';
import { useMutation, useQuery } from '@apollo/react-hooks';
import CloseIcon from '@material-ui/icons/Close';
import WarningIcon from '@material-ui/icons/Warning';
import CheckCircleIcon from '@material-ui/icons/CheckCircle';
import ErrorIcon from '@material-ui/icons/Error';
import InfoIcon from '@material-ui/icons/Info';
import { makeStyles } from '@material-ui/styles';
import Loader from '../../../components/Loader/Loader';
import { useHistory } from 'react-router-dom';
import toPascal from '../../../lib/toPascal';
import PostSyncTaskManager from '../../../components/PostSyncTaskManager/PostSyncTaskManager';

const UPDATE_RULE = gql`
  mutation($connectionRuleInput: connectionRuleInput!) {
    updateRule(connectionRule: $connectionRuleInput) {
      connectionID
      dateCreated
      enabled
      fieldMappings
      destinationContainerCfg
      name
      sourceContainerCfg
      sourcePostSyncTasks
      destinationPostSyncTasks
      syncIntervalCron
      lastUpdated
      lastExecutedStatus
      lastExecuted
      jsonSourceContainerCfg {
        containerID
      }
      id
      jsonFieldMappings {
        destField {
          id
          key
          title
        }
        options {
          alwaysUpdateFromSrc
          mutuallyExclusive
          nullAction
        }
        srcField {
          id
          key
          title
        }
      }
      jsonDestinationContainerCfg {
        containerID
      }
    }
  }
`;

const UPDATE_CURRENT_RULE = gql`
  mutation UpdateCurrentRule($id: Int!, $data: Object) {
    updateCurrentRule(id: $id, data: $data) @client
  }
`;

const GET_CURRENT_RULE = gql`
  query getCurrentRule($connectionId: ID) {
    connections(id: $connectionId) {
      name
      id
      destinationPlatformCfg
      jsonDestinationPlatformCfg {
        platformID
      }
      enabled
      lastUpdated
      sourcePlatformCfg
      connectionRules {
        dateCreated
        destinationContainerCfg
        enabled
        id
        sourcePostSyncTasks
        destinationPostSyncTasks
        lastExecuted
        lastExecutedStatus
        lastUpdated
        name
        sourceContainerCfg
        syncIntervalCron
        connectionID
        jsonDestinationContainerCfg {
          containerID
        }
        jsonSourceContainerCfg {
          containerID
        }
        jsonFieldMappings {
          srcField {
            id
            key
            title
          }
          destField {
            id
            key
            title
          }
          options {
            alwaysUpdateFromSrc
            mutuallyExclusive
            nullAction
            nullActionLabel
          }
        }
      }
    }
    sourceContainers(connectionId: $connectionId) {
      id
      name
    }
    destinationContainers(connectionId: $connectionId) {
      id
      name
    }
  }
`;

const GET_FIELD_CONFIG = gql`
  query getFieldConfig(
    $connectionId: ID
    $sourcecontainerId: ID
    $destinationcontainerId: ID
  ) {
    nullActions
    sourceFields(connectionId: $connectionId, containerId: $sourcecontainerId) {
      __typename
      id
      key
      title
    }
    destinationFields(
      connectionId: $connectionId
      containerId: $destinationcontainerId
    ) {
      __typename
      id
      key
      title
    }
  }
`;

const ADD_MAPPING = gql`
  mutation addMapping($id: Int!, $data: Object) {
    addMapping(id: $id, data: $data) @client
  }
`;

const useStyles = makeStyles(theme => ({
  paper: {
    width: '100%',
    marginTop: theme.spacing(4),
    position: 'relative',
    padding: theme.spacing(4),
  },
  pageTitle: {
    marginTop: theme.spacing(4),
    marginBottom: theme.spacing(4),
  },
  actionBar: { marginTop: theme.spacing(4), marginBottom: theme.spacing(4) },
  savebutton: {
    color: 'white',
  },
}));

function MappingContainer(props) {
  const user = window.userInfo;
  if (!user.IsConnectionAdmin) return "You shouldnt be here"
  
  const classes = useStyles();
  let history = useHistory();

  const [isDirty, setIsDirty] = useState(false);

  const [sourceContainerId, setSourceContainerId] = useState();
  const [destinationContainerId, setDestinationContainerId] = useState();

  const [currentConnection, setCurrentConnection] = useState({});
  const [currentRule, setCurrentRule] = useState({});

  const [toastMesage, setToastMesage] = useState('');
  const [updateStatus, setUpdateStatus] = useState('info');

  const variantIcon = {
    success: CheckCircleIcon,
    warning: WarningIcon,
    error: ErrorIcon,
    info: InfoIcon,
  };

  const Icon = variantIcon[updateStatus];

    const [updateRule, { updateData, updateLoading, updateError, updateCalled }] = useMutation(UPDATE_RULE,
    {
        onCompleted: data => {
            //       setUpdateStatus("success");
            // setToastMesage("Data has been successfully saved");
            // setOpen(true);
            props.websocket.send(`{ cmd: \"UpdateSchedules\"}`);
            history.push(`/connections/${props.match.params.connectionId}`);

            // refetch();
        },
        // update(cache, { data }) {

        //   console.log(data);
        //   console.log(cache);
        //   // console.log(data);

        //   const cacheValue = cache.readQuery({
        //     query: gql`
        //       query crules{
        //         connections {
        //           name
        //           id
        //         }
        //       } ` });

        //   console.log(cacheValue);
        // // console.log(data.createConnection);
        // cache.writeQuery({
        //   query: CONNECTIONS,
        //   data: { connections: cacheValue.connections.push(data.updateConnection) }
        // });
        // }
    });
    /*
    const handleUpdateRule = () => {
        
    };
    */

  const [open, setOpen] = React.useState(false);

  const handleClick = () => {
    setOpen(true);
  };

  const handleClose = (event, reason) => {
    if (reason === 'clickaway') {
      return;
    }

    setOpen(false);
  };

  // const [addMapping] = useMutation(ADD_MAPPING, {});

  const { loading, error, data, refetch } = useQuery(GET_CURRENT_RULE, {
    variables: {
      connectionId: props.match.params.connectionId,
    },
    onError: err => {
      // console.log(JSON.parse(err));
    },
    onCompleted: d => {
      // console.log(d);

      let cc = _.find(
        data.connections,
        o => o.id == props.match.params.connectionId
      );

      setCurrentConnection(cc);
    },
  });

  // console.log(data)

  useEffect(() => {
    let cr =
      _.find(
        currentConnection.connectionRules,
        o => o.id == props.match.params.ruleId
      ) || {};
    if (!_.isEmpty(cr)) {
      // console.log(cr);
      setCurrentRule(cr);

      if (cr.jsonSourceContainerCfg)
        setSourceContainerId(cr.jsonSourceContainerCfg.id);
      if (cr.jsonDestinationContainerCfg)
        setDestinationContainerId(cr.jsonDestinationContainerCfg.id);
    }
  }, [currentConnection]);

  // console.log(currentConnection);
  // console.log(currentRule);

  // const currentRule = _.find(
  //         currentConnection.connectionRules,
  //         o => o.id == props.match.params.ruleId
  //       ) || {};

  // console.log(currentRule.jsonDestinationContainerCfg); /"ed1923a2-baa6-4ac7-abe9-2e899922d373"
  //console.log(currentRule.jsonSourceContainerCfg); // "65a04f6e-cf34-4e68-97c5-d6dcdc305726"

  const fieldConfig = useQuery(GET_FIELD_CONFIG, {
    variables: {
      connectionId: props.match.params.connectionId,
      sourcecontainerId:
        currentRule.jsonSourceContainerCfg &&
        currentRule.jsonSourceContainerCfg.containerID,
      destinationcontainerId:
        currentRule.jsonDestinationContainerCfg &&
        currentRule.jsonDestinationContainerCfg.containerID,
    },
  });

  // console.log(props.match.params.connectionId);
  // console.log(currentRule.jsonSourceContainerCfg);
  // console.log(currentRule.jsonDestinationContainerCfg);

  // const [updateCurrentRule] = useMutation(UPDATE_CURRENT_RULE,{
  //   onCompleted:(data)=>{
  //     console.log(data)
  //     setOpen(true);
  // }});

  // console.log(fieldConfig);

  const addPostSyncTask = (task, tasksKey) => {
    // console.log(mapping);

    var newRule = Object.assign({}, currentRule);
    newRule[tasksKey].push(task);
    setCurrentRule(newRule);
    setIsDirty(true);
  };

  const updateTasks = (newTasks, tasksKey) => {
    var newRule = Object.assign({}, currentRule);
    newRule[tasksKey] = newTasks;
    setCurrentRule(newRule);
    setIsDirty(true);
  };

  const addMapping = mapping => {
    // console.log(mapping);

    if (mapping.destField.id.startsWith('ootb-')) {
      mapping.destField.id = '';
    }

    if (mapping.srcField.id.startsWith('ootb-')) {
      mapping.srcField.id = '';
    }

    var newRule = Object.assign({}, currentRule);
    newRule.jsonFieldMappings.push(mapping);
    setCurrentRule(newRule);
    setIsDirty(true);
  };

  const handleDeleteAction = (e, index) => {
    // console.log(index);
    var newRule = Object.assign({}, currentRule);
    // newRule.jsonFieldMappings = _.pullAt(newRule.jsonFieldMappings, [index]);
    newRule.jsonFieldMappings.splice(index, 1);
    setCurrentRule(newRule);
    setIsDirty(true);
  };

  const StringifyConfig = o => {
    return JSON.stringify(toPascal(o));
  };

  //converts ana actaul value to a pure string
  const configToString = (o,key) => {
      for (let index = 0; index < o.length; index++) {
        const element = o[index];
          element[key] = JSON.stringify(element[key]);
      }
      return StringifyConfig(o)
  }

  const saveRule = () => { 
    var newData = {
      id: currentRule.id,
      name: currentRule.name,
      enabled: currentRule.enabled,
      destinationContainerCfg: StringifyConfig(
        currentRule.jsonDestinationContainerCfg
      ),

      sourceContainerCfg: StringifyConfig(currentRule.jsonSourceContainerCfg),

      syncIntervalCron: currentRule.syncIntervalCron,
      fieldMappings: StringifyConfig(currentRule.jsonFieldMappings),
      destinationPostSyncTasks: configToString(
        currentRule.destinationPostSyncTasks,"Cfg"
      ),
      // destinationPostSyncTasks: StringifyConfig(
      //   currentRule.destinationPostSyncTasks
      // ),
      sourcePostSyncTasks: StringifyConfig(currentRule.sourcePostSyncTasks),

      destinationContainerCfg: StringifyConfig(
        currentRule.jsonDestinationContainerCfg
      ),

      connectionID: currentRule.connectionID,
    };

    // var x = toPascal(currentRule.jsonFieldMappings);

    //console.log(newData);

    //
    //destinationContainerCfg:data.currentrule.name
    //lastUpdated:data.currentrule.name
    //sourceContainerCfg:data.currentrule.name
    //syncIntervalCron:data.currentrule.name
    //fieldMappings:data.currentrule.name

    updateRule({
      variables: {
        connectionRuleInput: { ...newData },
      },
    });
    setIsDirty(false);
  };

  const handleDataChange = event => {
    var key = event.target.name;
    var val = event.target.value;
    // console.log(key, val);

    if (val == 'true' || val == 'false') {
      // console.log("should convert to bool");
      val = val != 'true';
    }

    let newVal = {};
    newVal[key] = val;

    let newCurrentRule = Object.assign({}, currentRule, newVal);
    setCurrentRule(newCurrentRule);
    setIsDirty(true);
  };

  if (error) {
    // console.log(error)
    return <pre>{JSON.stringify(error, null, 1)}</pre>;
  }

  if (data && !_.isEmpty(currentRule)) {
    // console.log(data)

    return (
      <Grid container item xs={12}>
        <Grid item xs={12}>
          <Breadcrumbs aria-label="breadcrumb">
            <Link color="inherit" href="/#/">
              Home
            </Link>
            <Link color="inherit" href="/#/connections">
              Connections
            </Link>
            <Link
              color="inherit"
              href={`/#/connections/${currentRule.connectionID}`}
            >
              {currentConnection.name}
            </Link>
            <Typography color="textPrimary">{`Edit ${currentRule.name}`}</Typography>
          </Breadcrumbs>
        </Grid>

        <Grid item xs={12}>
          <Typography variant="h2" className={classes.pageTitle}>
            {`Edit ${currentRule.name}`}
          </Typography>
        </Grid>

        {/* <pre>{JSON.stringify(currentRule, null, 1)}</pre> */}

        <Paper className={classes.paper}>
          <Grid container item xs={12}>
            <ManageRule
              {...props}
              rule={currentRule}
              setIsDirty={setIsDirty}
              updateHandler={handleDataChange}
              sourceContainers={data.sourceContainers}
              destinationContainers={data.destinationContainers}
              setSourceContainerId={setSourceContainerId}
              setDestinationContainerId={setDestinationContainerId}
            />
          </Grid>

          {currentRule.jsonSourceContainerCfg &&
            currentRule.jsonSourceContainerCfg.containerID &&
            currentRule.jsonDestinationContainerCfg &&
            currentRule.jsonDestinationContainerCfg.containerID && (
              <Grid container item xs={12}>
                <MappingsBuilder
                  {...props}
                  rule={currentRule}
                  setIsDirty={setIsDirty}
                  deleteAction={handleDeleteAction}
                />
              </Grid>
            )}
          {currentRule.jsonSourceContainerCfg &&
            currentRule.jsonSourceContainerCfg.containerID &&
            currentRule.jsonDestinationContainerCfg &&
            currentRule.jsonDestinationContainerCfg.containerID && (
              <Grid container item xs={12}>
                <NewMapping
                  ruleId={props.match.params.ruleId}
                  addMapping={addMapping}
                  nullActions={fieldConfig.data.nullActions}
                  sourceFields={fieldConfig.data.sourceFields}
                  destinationFields={fieldConfig.data.destinationFields}
                />
              </Grid>
            )}

          <Grid container item xs={12}>
            <PostSyncTaskManager
              addPostSyncTask={addPostSyncTask}
              updateTasks={updateTasks}
              tasks={currentRule.destinationPostSyncTasks}
              tasksKey={'destinationPostSyncTasks'}
              platformId={
                currentConnection.jsonDestinationPlatformCfg.platformID
              }
            />
          </Grid>

          <Grid
            container
            item
            xs={12}
            container
            alignItems="center"
            className={classes.actionBar}
          >
            <Button
              variant="contained"
              color="primary"
              className={classes.savebutton}
              disabled={!isDirty}
              onClick={saveRule}
            >
              Save
            </Button>
          </Grid>
        </Paper>
        {/* <pre>
          {JSON.stringify(fieldConfig.data.sourceFields, null, 1)}
        </pre> */}

        <div>
          <Snackbar
            anchorOrigin={{
              vertical: 'top',
              horizontal: 'right',
            }}
            open={open}
            autoHideDuration={6000}
            onClose={handleClose}
            ContentProps={{
              'aria-describedby': 'message-id',
            }}
            message={
              <span id="message-id">
                <Icon />
                {toastMesage}
              </span>
            }
            action={[
              <IconButton
                key="close"
                aria-label="close"
                color="inherit"
                onClick={handleClose}
              >
                <CloseIcon />
              </IconButton>,
            ]}
          />
        </div>
      </Grid>
    );
  }

  return <Loader />;
}

export default MappingContainer;
