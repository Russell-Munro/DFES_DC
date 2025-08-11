import React, { PureComponent, useState } from 'react';

import { Link } from 'react-router-dom';
import { useHistory } from 'react-router-dom';

import {
  IconButton,
  Typography,
  Grid,
  Menu,
  MenuItem,
  Divider,
  Fade,
} from '@material-ui/core';
import SyncIcon from '@material-ui/icons/Sync';
import MoreVertIcon from '@material-ui/icons/MoreVert';
import { makeStyles } from '@material-ui/styles';
import gql from 'graphql-tag';
import { useMutation } from '@apollo/react-hooks';
import toPascal from '../../lib/toPascal';

const useStyles = makeStyles(theme => ({
  connectionWrapper: {
    padding: theme.spacing(2),
  },
  clickable: {
    cursor: 'pointer',
  },
}));

function Connection(props) {
  //console.log(props);

  const classes = useStyles();
  const user = window.userInfo;

  const [menuAnchorEl, setMenuAnchorEl] = useState(null);
  let history = useHistory();

  const handleDelete = event => {
    props.handleDelete(props.id);
  };

  // const [
  //   updateRule,
  //   { updateData, updateLoading, updateError, updateCalled }
  // ] = useMutation(DELETE_CONNECTION, {
  //   onCompleted: data => {
  //     setUpdateStatus("success");
  //     setToastMesage("Data has been successfully saved");
  //     setOpen(true);
  //   }
  // });

  const handleMenuClick = event => {
    console.log(event.currentTarget);

    setMenuAnchorEl(event.currentTarget);
  };

  const handleMenuSelect = path => {
    //console.log(path);
    setMenuAnchorEl(null);
  };

  const handleManageSelect = () => {
    props.setCurrentConnection(
      Object.assign(
        {},
        {
          name: props.Name,
          id: props.id,
          enabled: props.enabled ? props.enabled : false,
        }
      )
    );

    props.manageConnection();
  };

  const handleMenuClose = () => {
    setMenuAnchorEl(null);
  };

  const StringifyConfig = o => {
    return JSON.stringify(toPascal(o));
  };

  const updateConnection = o => {
    // console.log("props");
    // console.log(props);

    var conn = {
      name: props.name,
      sourcePlatformCfg: StringifyConfig(props.jsonSourcePlatformCfg),
      destinationPlatformCfg: StringifyConfig(props.jsonDestinationPlatformCfg),
      id: props.id,
      enabled: false,
    };

    var returnedConn = Object.assign({}, conn, o);

    // console.log(returnedConn);

    props.handleUpdate({
      variables: {
        connection: returnedConn,
      },
    });
  };

  const isConfigured = () => {
    // console.log(props.jsonSourcePlatformCfg.integratorID,props.jsonDestinationPlatformCfg.integratorID);

    const hasSourceConfig =
      props.jsonSourcePlatformCfg &&
      props.jsonSourcePlatformCfg.integratorID != null &&
      props.jsonSourcePlatformCfg.integratorID != '';
    const hasDestConfig =
      props.jsonDestinationPlatformCfg &&
      props.jsonDestinationPlatformCfg.integratorID != null &&
      props.jsonDestinationPlatformCfg.integratorID != '';
    // console.log(hasSourceConfig && hasDestConfig)
    return hasSourceConfig && hasDestConfig;
  };

  const navigateTo = () => {
    if (!isConfigured()) {
      alert('Connection must be configured first');
    } else {
      history.push(`/connections/${props.id}/`);
    }
  };

  // console.log('-----------');
  // console.log(window.userInfo);

  return (
    <div className={classes.connectionWrapper}>
      <Grid container item xs={12} spacing={10} alignItems={'center'}>
        <Grid item xs={1} container justify={'center'}>
          <div onClick={navigateTo} className={classes.clickable}>
            <SyncIcon fontSize="small" />
          </div>
        </Grid>
        <Grid item xs={8}>
          <div onClick={navigateTo} className={classes.clickable}>
            <Typography variant={'subtitle1'}>
              {props.name}
              {!isConfigured() && ` (Not configured)`}
            </Typography>
          </div>
        </Grid>
        <Grid item xs={2}>
          <div onClick={navigateTo} className={classes.clickable}>
            <Typography variant="subtitle1" color="textSecondary">
              {props.enabled ? 'Enabled' : ''}
            </Typography>
          </div>
        </Grid>

        <Grid item xs={1}>
          <IconButton
            aria-label="more"
            aria-controls="long-menu"
            aria-haspopup="true"
            onClick={handleMenuClick}
          >
            <MoreVertIcon />
          </IconButton>
          <Menu
            id="action-menu"
            anchorEl={menuAnchorEl}
            keepMounted
            open={Boolean(menuAnchorEl)}
            onClose={handleMenuClose}
          >
            <MenuItem
              key="Manage"
              onClick={navigateTo}
              disabled={!isConfigured()}
            >
              Manage
            </MenuItem>

            {user.IsConnectionAdmin && (
              <MenuItem
                key="configure"
                onClick={e => history.push(`/connection/${props.id}/edit`)}
              >
                Configure
              </MenuItem>
            )}

            {user.IsConnectionAdmin && props.enabled && (
              <MenuItem
                key="Disable"
                disabled={!isConfigured()}
                onClick={() => updateConnection({ enabled: false })}
              >
                Disable
              </MenuItem>
            )}

            {user.IsConnectionAdmin && !props.enabled && (
              <MenuItem
                key="Enabled"
                disabled={!isConfigured()}
                onClick={() => updateConnection({ enabled: true })}
              >
                Enable
              </MenuItem>
            )}

            {user.IsConnectionAdmin && <Divider />}
            {user.IsConnectionAdmin && (
              <MenuItem
                //  className={classes.destructive}
                key="Remove"
                onClick={handleDelete}
              >
                Remove
              </MenuItem>
            )}
          </Menu>
        </Grid>
      </Grid>
    </div>
  );
}
export default Connection;
