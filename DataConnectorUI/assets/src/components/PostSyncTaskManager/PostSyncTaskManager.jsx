import React, { useState, useEffect } from 'react';
import PropTypes from 'prop-types';
import gql from 'graphql-tag';
import Loader from '../Loader/Loader';
import { useQuery } from '@apollo/react-hooks';
import {
  Typography,
  makeStyles,
  Select,
  MenuItem,
  IconButton,
  Grid,
  TextField,
} from '@material-ui/core';
import AddIcon from '@material-ui/icons/Add';
import DeleteOutlineIcon from '@material-ui/icons/DeleteOutline';

//import { Test } from './PostSyncTaskManager.styles';

const GET_POST_SYNC_TASKS = gql`
  query postSyncTasks($platformId: ID) {
    postSyncTasks(platformId: $platformId) {
      name
      postSyncTaskID
      schema
    }
  }
`;

const useStyles = makeStyles(theme => ({
  title: {
    marginTop: theme.spacing(10),
    marginBottom: theme.spacing(4),
  },
  taskName: {
    marginRight:10,
    borderRadius: theme.shape.borderRadius,
    padding: theme.spacing(4),
    borderRadius: theme.shape.borderRadius,
    backgroundColor: theme.palette.grey['200'],
    width: '100%',
  },
  select: {
    // backgroundColor: theme.palette.grey['200'],
    padding: theme.spacing(2),
    marginRight:10,
    marginTop: theme.spacing(6),
    marginBottom: theme.spacing(1),
    borderRadius: theme.shape.borderRadius,
  },
}));

const PostSyncTaskManager = props => {
  const classes = useStyles();

  const [task, setTask] = useState({});
  const [taskConfigs, setTaskConfigs] = useState();

  useEffect(() => {
    setTaskConfigs(props.tasks);
  }, [props.tasks]);

  const { loading, error, data, refetch } = useQuery(GET_POST_SYNC_TASKS, {
    variables: {
      platformId: props.platformId,
    },
    onError: err => {
      console.log(JSON.parse(err));
    },
    onCompleted: d => {},
  });

  const handleNewTask = t => {
    var newtask = Object.assign(
      {},
      { Cfg: t.schema },
      { PlatformID: props.platformId, PostSyncTaskID: t.postSyncTaskID }
    );
    let newTasks = [...taskConfigs, newtask];

    setTaskConfigs(newTasks);

    if (t.schema == null) {
        props.updateTasks(newTasks, props.tasksKey);
    }
  };

  const TaskName = myProps => {
    let t = _.find(data.postSyncTasks, o => {
      return o.postSyncTaskID == myProps.id;
    });

    return t.name;
  };

  const deleteTask = (e, idx) => {
    let newTasks = [...taskConfigs];

    setTaskConfigs(newTasks);
    newTasks.splice(idx, 1);
    props.updateTasks(newTasks, props.tasksKey);
  };

  const updateConfig = (e, key, index) => {
    let newTasks = [...taskConfigs];

    /////////////////////////
    console.log("test: ");
    console.log(e.target.value);
    /////////////////////////

    newTasks[index]['Cfg'][key] = e.target.value;
    setTaskConfigs(newTasks);
    props.updateTasks(newTasks, props.tasksKey);
  };

  if (error) {
    return <pre>{JSON.stringify(error, null, 1)}</pre>;
  }

  return (
    <Grid container xs={12} item>
      <Typography variant="h5" className={classes.title}>
        Post Sync Tasks
      </Typography>
      {/* <pre>{JSON.stringify(taskConfigs, null, 1)}</pre> */}
      {data &&
        taskConfigs &&
        taskConfigs.map((t, idx) => {

          console.log(t.Cfg);
          if(t.Cfg && typeof t.Cfg == "string"){
            t.Cfg = JSON.parse(t.Cfg);

          }

          return (
            <Grid item container xs={12} key={idx} alignItems="center">
              <Grid item xs={4} container alignItems="center">
                <Typography variant="body1" className={classes.taskName}>
                  <TaskName id={t.PostSyncTaskID} />
                </Typography>
              </Grid>

                  {t.Cfg && t.Cfg != null &&  Object.keys(t.Cfg).map((key, index) => {
                  return (
                    <Grid
                      item
                      xs={4}
                      key={`${idx}-${index}-field`}
                      container
                      alignItems="center"
                    >
                      <TextField
                        id="standard-required"
                        label={key}
                        fullWidth
                        defaultValue={t.Cfg[key]}
                        onChange={e => {
                          updateConfig(e, key, idx);
                        }}
                        className={classes.textField}
                        margin="normal"
                      />
                    </Grid>
                  );
                })}

              <Grid item xs={1} container alignItems="center" justify="center">
                <DeleteOutlineIcon onClick={e => deleteTask(e, idx)} />
              </Grid>
            </Grid>
          );
        })}
      {data && (
        <Grid item container xs={12}>
          <Grid item container xs={4}>
            <Select
              fullWidth
              value={task}
              className={classes.select}
              renderValue={selected => {
                // console.log(selected);
                if (_.isEmpty(selected)) {
                  return <em>Add a new task</em>;
                }
                return `${selected.name}`;
              }}
              onChange={e => {
                //setTask(e.target.value);
                handleNewTask(e.target.value);
              }}
              inputProps={{
                name: `new-source`,
                id: `new-source`,
              }}
            >
              <MenuItem disabled value="">
                <em>Add a new task</em>
              </MenuItem>

              {data.postSyncTasks.map((task, idx) => (
                <MenuItem key={`new-task-${idx}`} value={task}>
                  {task.name}
                </MenuItem>
              ))}
            </Select>
          </Grid>
          <Grid
            item
            container
            xs={1}
            alignItems="center"
            justify="center"
          ></Grid>
        </Grid>
      )}
    </Grid>
  );

  return <Loader />;
};

export default PostSyncTaskManager;
