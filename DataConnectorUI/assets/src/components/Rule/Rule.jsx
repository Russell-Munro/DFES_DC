import React, { useState, useEffect } from 'react';
import {
  Typography,
  Grid,
  Fab,
  MenuItem,
  IconButton,
  Menu,
  makeStyles,
  LinearProgress,
  CircularProgress,
  Divider,
} from '@material-ui/core';

import WrapTextIcon from '@material-ui/icons/WrapText';
import PlayArrowIcon from '@material-ui/icons/PlayArrow';
import MoreVertIcon from '@material-ui/icons/MoreVert';
import { useHistory } from 'react-router-dom';
import TimeAgo from 'javascript-time-ago';
import en from 'javascript-time-ago/locale/en';
TimeAgo.addLocale(en);
const timeAgo = new TimeAgo('en-US');
import { CRON_PATTERNS } from '../../constants';
import gql from 'graphql-tag';
import { useMutation } from '@apollo/react-hooks';
import Transcript from './Transcript';
import Status from './Status';
import toPascal from '../../lib/toPascal';

const useStyles = makeStyles(theme => ({
  ruleWrapper: {
    padding: theme.spacing(4),
    borderBottom: '1px solid rgba(0,0,0,0.12)',
    position: 'relative',
  },
  actions: {
    position: 'absolute',
    top: 10,
    right: 10,
  },
  pageTitle: {
    marginTop: theme.spacing(10),
    marginBottom: theme.spacing(10),
  },
  success: {
    color: theme.palette.primary.dark,
  },
  error: {
    color: theme.palette.error.dark,
  },
  transcriptbutton: {
    marginTop: 10,
  },
}));

function Rule(props) {
  // console.log(props);
  const classes = useStyles();
  let history = useHistory();
  const [menuAnchorEl, setMenuAnchorEl] = useState(null);
  const [docsSynced, setDocsSynced] = useState(0);
  // const docsSyncedRef = useRef(docsSynced);
  const [totalDocs, setTotalDocs] = useState(124);
  const [isSyncing, setIsSyncing] = useState(false);
  const [statusMessage, setStatusMesage] = useState('');
  const user = window.userInfo;

  const handleMenuSelect = path => {
    history.push(path);
    setMenuAnchorEl(null);
  };

  var lastRun = 'Never run';
  if (props.lastExecuted != null) {
    lastRun = timeAgo.format(new Date(props.lastExecuted + 'Z'));
  }
  lastRun = lastRun == '50 years ago' ? 'Never run' : lastRun;

  const StringifyConfig = o => {
    return JSON.stringify(toPascal(o));
  };

  const configToString = (o,key) => {
    // console.log(o)
    for (let index = 0; index < o.length; index++) {
      const element = o[index];
      // console.log(element[key]);
      element[key] = JSON.stringify(element[key]);
      
    }
    // console.log(o)

    return StringifyConfig(o)

}


  const handleUpdateRule = delta => {
    var currentRule = {
      id: props.id,
      name: props.name,
      enabled: props.enabled,
      destinationContainerCfg: StringifyConfig(
        props.jsonDestinationContainerCfg
      ),
      sourceContainerCfg: StringifyConfig(props.jsonSourceContainerCfg),
      syncIntervalCron: props.syncIntervalCron,
      fieldMappings: props.fieldMappings,
      connectionID: props.connectionID,
      destinationPostSyncTasks: configToString(
        props.destinationPostSyncTasks,"Cfg"
      ),
      // destinationPostSyncTasks: StringifyConfig(
      //   currentRule.destinationPostSyncTasks
      // ),
      sourcePostSyncTasks: StringifyConfig(props.sourcePostSyncTasks),
    };

    var newRule = Object.assign({}, currentRule, delta);

    props.updateRule({
      variables: {
        connectionRuleInput: { ...newRule },
      },
    });
  };

  useEffect(() => {
    if (props.connectionRulesSyncing.indexOf(props.id) >= 0) {
      if (isSyncing != true) {
        //refetch
        setIsSyncing(true);
      }
    } else {
      if (isSyncing != false) {
        //refetch
        setIsSyncing(false);
      }
    }

    // if (props.statusMessage.DocStats) {
    //   setTotalDocs(props.statusMessage.DocStats.Total);
    //   setDocsSynced(props.statusMessage.DocStats.Current);
    // }
    // setStatusMesage(props.statusMessage.SourceDesc);
    // //      console.group(`should be showing for ${props.id}`)
    // }
    // }
  }, [props.connectionRulesSyncing]);

  const startSync = () => {
    if (isSyncing) {
      setIsSyncing(false);
      props.websocket.close();
    } else {
      setIsSyncing(true);
      props.websocket.send(`{ cmd: \"ExecSync\", args: [${props.id}] }`);
    }
  };

  //console.log(props);
  const lastExecutedStatus = JSON.parse(props.lastExecutedStatus);

  const ScheduleLabel = () => {
    var schedule = _.find(CRON_PATTERNS, o => {
      //console.log(o.pattern,props.syncIntervalCron)
      return o.pattern == props.syncIntervalCron;
    });

    if (schedule) {
      return schedule.label;
    }

    return '';
  };

  const StatusLabel = myProps => {
    //console.log(myProps);

    var icon = '';
    var cls = '';

    switch (myProps.label) {
      case 'Completed Successfully':
        // icon = <CheckCircleIcon fontSize={"small"} color="primary" />;
        cls = 'success';
        break;
      case 'Sync Failed':
        // icon = <ErrorIcon fontSize={"small"} color="error" />;
        cls = 'error';
        break;
      default:
        // icon = <CheckCircleIcon fontSize={"small"} />;
        break;
    }

    return (
      <Grid container direction="row" justify="flex-start" alignItems="center">
        {/* <Grid item>{icon}</Grid> */}
        <Grid item>
          <Typography variant="caption" className={classes[cls]}>{`${
            myProps.ExecutionStatus != null ? myProps.ExecutionStatus : ``
          }`}</Typography>
        </Grid>
      </Grid>
    );
  };

  // console.log(!props.enabled && );

  return (
    <Grid container item xs={12} className={classes.ruleWrapper}>
      <Grid item xs={1} container direction="column" alignContent="center">
        {!isSyncing && (
          <Fab
            onClick={startSync}
            disabled={
              !(
                user.IsConnectionAdmin &&
                props.enabled &&
                props.webSocketState == WebSocket.OPEN
              )
            }
            size={'small'}
          >
            <PlayArrowIcon />
          </Fab>
        )}
        {isSyncing && <CircularProgress disableShrink />}

        {isSyncing && (
          <IconButton
            className={classes.transcriptbutton}
            size="small"
            disabled={!isSyncing}
            onClick={() => props.setOpenTranscript(true)}
            aria-label="transcript"
          >
            <WrapTextIcon size="small" />
          </IconButton>
        )}
      </Grid>

      <Grid item xs={3} container>
        <Grid item xs container spacing={2} direction="column">
          <Typography variant="subtitle2">{props.name}</Typography>
          <Typography variant="body2">
            {props.enabled ? 'Enabled' : 'Disabled'}
          </Typography>
        </Grid>
      </Grid>

      <Grid item container xs={3} direction="row">
        <Grid item>
          {!isSyncing && <Typography variant="body2">{lastRun}</Typography>}
          {isSyncing && <Typography variant="body2">Syncing now</Typography>}
        </Grid>

        <Grid item container>
          {isSyncing && <Status />}
          {!isSyncing && <StatusLabel {...lastExecutedStatus} />}
          {/* <pre>{JSON.stringify(props, null, 1)}</pre> */}
        </Grid>
      </Grid>

      <Grid item xs={5} container>
        <Grid item xs={4} container>
          <Typography variant="body2">
            {props.jsonSourceContainerCfg &&
              props.jsonSourceContainerCfg.containerID &&
              props.sourceFolder(props.jsonSourceContainerCfg.containerID).name}
          </Typography>
        </Grid>

        <Grid item xs={4} container>
          <Typography variant="body2">
            {props.jsonDestinationContainerCfg &&
              props.jsonDestinationContainerCfg.containerID &&
              props.destinationFolder(
                props.jsonDestinationContainerCfg.containerID
              ).name}
          </Typography>
        </Grid>

        <Grid item xs={4} container>
          <Typography variant="body2">
            <ScheduleLabel />
          </Typography>
        </Grid>
      </Grid>

      <IconButton
        className={classes.actions}
        aria-label="more"
        aria-controls="long-menu"
        aria-haspopup="true"
        onClick={e => setMenuAnchorEl(e.currentTarget)}
      >
        <MoreVertIcon />
      </IconButton>
      <Menu
        id="managemenu"
        anchorEl={menuAnchorEl}
        keepMounted
        open={Boolean(menuAnchorEl)}
        onClose={e => setMenuAnchorEl(null)}
      >
        {user.IsConnectionAdmin && (
          <MenuItem
            onClick={() =>
              handleMenuSelect(`/connections/${props.connectionId}/${props.id}`)
            }
          >
            Configure
          </MenuItem>
        )}

        {user.IsConnectionAdmin && props.enabled && (
          <MenuItem onClick={e => handleUpdateRule({ enabled: false })}>
            Disable
          </MenuItem>
        )}

        {!props.enabled && user.IsConnectionAdmin && (
          <MenuItem onClick={e => handleUpdateRule({ enabled: true })}>
            Enable
          </MenuItem>
        )}
        <MenuItem key="logs" onClick={e => history.push(`/logs/${props.id}`)}>
          Logs
        </MenuItem>
        {user.IsConnectionAdmin && (
         
            <Divider />
        )}
        {user.IsConnectionAdmin && (

            <MenuItem
              //  className={classes.destructive}
              key="Remove"
              onClick={e =>
                props.deleteRule({ variables: { connectionRuleId: props.id } })
              }
            >
              Remove
            </MenuItem>
       
        )}
      </Menu>

      {/* 
      
        {isSyncing && <Transcript transcript={props.statusTranscript} />}

       {transcript.map((t)=>{
             
            //  {"LogType":1,"LogAction":2,"LogResult":0,"Source":"ExecuteRuleSync","SourceDesc":"Resolving SyncField Mappings against originating platforms","Msg":"","Exception":null,"Data":null,"TimeStamp":"2019-11-08T01:04:13.3473249Z","DocStats":null,"connectionRuleID":2}

            <p>{t.SourceDesc}</p>

          })} 
      </Grid> */}
    </Grid>
  );
}

export default React.memo(Rule);
