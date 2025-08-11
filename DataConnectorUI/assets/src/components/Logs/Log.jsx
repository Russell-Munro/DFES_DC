import React, { useState } from "react";
import {
  makeStyles,
  Grid,
  Breadcrumbs,
  Link,
  Typography,
  Paper,
  Table,
  TableHead,
  TableRow,
  TableCell,
  TableBody,
  TablePagination,
  Select,
  MenuItem,
  FormControlLabel,
  Checkbox,
  IconButton,
  Modal,
  Chip,
  Tab,
  Divider
} from "@material-ui/core";
import ErrorIcon from "@material-ui/icons/Error";
import gql from "graphql-tag";
import { useQuery } from "@apollo/react-hooks";
import Loader from "../Loader/Loader";
import FormattedDate from "../FormattedDate/FormattedDate";
import { useHistory } from "react-router-dom";

import SubjectIcon from "@material-ui/icons/Subject";
import WarningIcon from "@material-ui/icons/Warning";

const useStyles = makeStyles(theme => ({
  paper: {
    width: "100%",
    marginTop: theme.spacing(4),
    position: "relative",
    padding: theme.spacing(4)
  },
  pageTitle: {
    marginTop: theme.spacing(4),
    marginBottom: theme.spacing(4)
  },
  actionBar: { marginTop: theme.spacing(4), marginBottom: theme.spacing(4) },
  tableText: {
    fontSize: "12px"
  },
  error: {
    backgroundColor: "rgba(229,115,115,0.2)"
  },

  warning: {
    backgroundColor: "rgba(248,165,81,0.1)"
  },

  errorChip: {
    borderWidth: 2,
    borderColor: theme.palette.error.dark,
    backgroundColor: theme.palette.error.dark,
    color: "white",
    marginRight: 10
  },

  warningChip: {
    borderWidth: 2,
    borderColor: "#fd9d3b",
    backgroundColor: "#fd9d3b",
    marginRight: 10,
    color: "white"
  },

  log: {
    position: "absolute",
    width: "80%",
    height: "80%",
    top: "10%",
    bottom: "10%",
    left: "10%",
    right: "10%",
    border: 0,
    overflow: "auto"
  },
  summaryTable: {
    width: "100%",
    marginTop: 20,
    marginBottom: 20
  },
  tableCount: {
    textAlign: "center"
  },
  summaryTitle: {
    marginTop: 20,
    marginBottom: 20
  },
  selectWrapper: {
    marginTop: 10
  }
}));

const LOG = gql`
  query getLog($logId: ID, $connectionRuleId: ID) {
    logs(connectionRuleId: $connectionRuleId) {
      id
      dateCreated
    }
    log(dataConnectorLogID: $logId) {
      id
      connectionRuleName
      logAction
      logType
      logResult
      message
      dateCreated
      connectionRuleID
      syncTimeElapsed
      stats
      syncLog {
        exception
        msg
        source
        sourceDesc
        logAction
        logResult
        logType
        timeStamp
      }
    }
  }
`;

function Log(props) {
  const classes = useStyles();

  const [rowsPerPage, setRowsPerPage] = useState(100);
  const [page, setPage] = useState(0);

  const [showWarnings, setShowWarnings] = useState(true);
  const [showErrors, setShowErrors] = useState(true);
  const [showTrace, setShowTrace] = useState(false);
  const [showTraceWindow, setShowTraceWindow] = useState(false);

  const [exceptionMsg, setExceptionMsg] = useState();

  let history = useHistory();

  const { loading, error, data } = useQuery(LOG, {
    variables: {
      logId: props.match.params.logId,
      connectionRuleId: props.match.params.connectionRuleId
    }
  });

  console.log(props.match.params.connectionRuleId);

  const currentLog = data && data.log.length > 0 && data.log[0];

  const filteredLogs = () => {
    var typeArr = [];
    if (showErrors) typeArr.push("Error");
    if (showWarnings) typeArr.push("Warning");
    if (showTrace) typeArr.push("Trace");

    var results = _.filter(currentLog.syncLog, function(o) {
      return typeArr.indexOf(o.logType) >= 0;
    });

    return results;
  };

  //   return currentLog.syncLog;
  // };

  console.log(currentLog);

  const FormattedTime = date => {
    var d = new Date(date.date);
    //console.log(date)
    return `${d.getHours()}:${d.getMinutes()}:${d.getSeconds()}:${d.getUTCMilliseconds()}`;
  };

  const LogEntry = log => {
    console.log(log);
    // console.log(typeof syncLogs);
    // console.log(JSON.stringify(syncLogs));

    return filteredLogs()
      .slice(page * rowsPerPage, page * rowsPerPage + rowsPerPage)
      .map((logentry, idx) => {
        return (
          <TableRow
            key={`logentry-${idx}`}
            style={{ height: 33 }}
            className={classes[logentry.logType.toLocaleLowerCase()]}
          >
            <TableCell className={classes.tableText}>
              {`${logentry.logType}`}
            </TableCell>
            <TableCell className={classes.tableText}>
              {logentry.source}
            </TableCell>

            <TableCell className={classes.tableText}>
              {logentry.sourceDesc}
            </TableCell>
            <TableCell className={classes.tableText}>
              {logentry.msg}

              {logentry.exception && logentry.exception.length <= 500 && (
                <div>{logentry.exception}</div>
              )}
            </TableCell>
            <TableCell className={classes.tableText}>
              {logentry.exception && logentry.exception.length > 500 && (
                <IconButton
                  aria-label="Show trace log"
                  onClick={e => {
                    setExceptionMsg(logentry.exception);
                    setShowTraceWindow(true);
                  }}
                >
                  <SubjectIcon />
                </IconButton>
              )}
            </TableCell>
            <TableCell className={classes.tableText}>
              <FormattedTime date={logentry.timeStamp} />
            </TableCell>
          </TableRow>
        );
      });
  };

  const handleChangePage = (event, newPage) => {
    setPage(newPage);
  };

  const handleChangeRowsPerPage = event => {
    setRowsPerPage(parseInt(event.target.value, 10));
    setPage(0);
  };

  const handleLogChange = e => {};

  console.log(currentLog);
  if (currentLog && currentLog.syncLog) {
    return (
      <Grid container item xs={12}>
        <Grid item xs={12}>
          <Breadcrumbs aria-label="breadcrumb">
            <Link color="inherit" href="/#/">
              Home
            </Link>
            <Link color="inherit" href="/#/logs">
              Logs
            </Link>
            <Typography color="textPrimary">
              {currentLog.connectionRuleName} &mdash;{" "}
              <FormattedDate date={currentLog.dateCreated} />
            </Typography>
          </Breadcrumbs>
        </Grid>

        <Grid item xs={12}>
          <Typography variant="h6" className={classes.pageTitle}>
            Log: {currentLog.connectionRuleName} &mdash;{" "}
            <FormattedDate date={currentLog.dateCreated} />
          </Typography>
        </Grid>

        <Paper className={classes.paper}>
          <Grid container item xs={12}>
            <Grid item xs={8}>
              <Typography variant="h4">
                {currentLog.stats.ExecutionStatus}
              </Typography>

              <Typography variant={"subtitle2"}>
                {`A sync was run on the connection rule ${currentLog.connectionRuleName} ${currentLog.syncTimeElapsed != null ? `taking ${currentLog.syncTimeElapsed}` : ``}. A total of ${currentLog.stats.BinaryTransferedBytes > 0 ? `${((currentLog.stats.BinaryTransferedBytes / 1024) / 1024).toFixed(2)}`:`0` } MB was transferred.`}
              </Typography>
            </Grid>
            <Grid item xs={4} className={classes.selectWrapper}>
              <Select
                label="demo-simple-select-label"
                id="demo-simple-select"
                fullWidth
                value={currentLog.id}
                onChange={e =>
                  history.push(
                    `/logs/${props.match.params.connectionRuleId}/${e.target.value}`
                  )
                }
              >
                {console.log(data.logs)}
                {data.logs.map((o, idx) => (
                  <MenuItem value={o.id} key={`menu-${idx}`}>
                    <FormattedDate date={o.dateCreated} />
                  </MenuItem>
                ))}
              </Select>
            </Grid>
            <Grid container item xs={12}>
              <Grid container item xs={12} className={classes.summaryTitle}>


                <Grid item xs={8}>
                  {" "}
                  <Typography variant="h5">Summary of results</Typography>
                </Grid>
                <Grid
                  item
                  xs={4}
                  container
                  justify="flex-end"
                  alignItems="flex-end"
                >
                  <Chip
                    icon={<ErrorIcon style={{ color: "white" }} />}
                    label={`${currentLog.stats.Errors} errors`}
                    className={classes.errorChip}
                  />

                  <Chip
                    icon={<WarningIcon style={{ color: "white" }} />}
                    label={`${currentLog.stats.Warnings} warnings`}
                    className={classes.warningChip}
                  />
                </Grid>
              </Grid>

              <Grid item xs={12}>
                <Grid
                  container
                  item
                  xs={12}
                  justify="center"
                  alignContent="center"
                >
                  <Table className={classes.summaryTable}>
                    <TableHead>
                      <TableRow>
                        <TableCell
                          className={classes.tableCount}
                          component="th"
                          scope="row"
                        ></TableCell>
                        <TableCell
                          className={classes.tableCount}
                          component="th"
                        >
                          Created
                        </TableCell>
                        <TableCell
                          className={classes.tableCount}
                          component="th"
                        >
                          Updated
                        </TableCell>
                        <TableCell
                          className={classes.tableCount}
                          component="th"
                        >
                          Skipped
                        </TableCell>
                        <TableCell
                          className={classes.tableCount}
                          component="th"
                        >
                          Deleted
                        </TableCell>
                      </TableRow>
                    </TableHead>
                    <TableBody>
                      <TableRow>
                        <TableCell component="th" scope="row">
                          Tags
                        </TableCell>
                        <TableCell className={classes.tableCount}>
                          {currentLog.stats.TagsCreated}
                        </TableCell>
                        <TableCell className={classes.tableCount}>
                          {currentLog.stats.TagsUpdated}
                        </TableCell>
                        <TableCell className={classes.tableCount}>
                          {currentLog.stats.TagsSkipped}
                        </TableCell>
                        <TableCell className={classes.tableCount}>
                          {currentLog.stats.TagsDeleted}
                        </TableCell>
                      </TableRow>
                      <TableRow>
                        <TableCell component="th">Containers</TableCell>
                        <TableCell className={classes.tableCount}>
                          {currentLog.stats.ContainersCreated}
                        </TableCell>
                        <TableCell className={classes.tableCount}>
                          {currentLog.stats.ContainersUpdated}
                        </TableCell>
                        <TableCell className={classes.tableCount}>
                          {currentLog.stats.ContainersSkipped}
                        </TableCell>
                        <TableCell className={classes.tableCount}>
                          {currentLog.stats.ContainersDeleted}
                        </TableCell>
                      </TableRow>
                      <TableRow>
                        <TableCell component="th">Objects</TableCell>
                        <TableCell className={classes.tableCount}>
                          {currentLog.stats.ObjectsCreated}
                        </TableCell>
                        <TableCell className={classes.tableCount}>
                          {currentLog.stats.ObjectsUpdated}
                        </TableCell>
                        <TableCell className={classes.tableCount}>
                          {currentLog.stats.ObjectsSkipped}
                        </TableCell>
                        <TableCell className={classes.tableCount}>
                          {currentLog.stats.ObjectsDeleted}
                        </TableCell>
                      </TableRow>
                    </TableBody>
                  </Table>
                </Grid>
              </Grid>
            </Grid>
          </Grid>
          <Grid container item xs={12}>
            <Typography variant="h5">Detailed Log</Typography>

            <Grid item xs={12}>
              <FormControlLabel
                control={
                  <Checkbox
                    checked={showErrors}
                    onChange={e => {
                      setShowErrors(!showErrors);
                      setPage(0);
                    }}
                    value="errorsOnly"
                  />
                }
                label="Show errors logs"
              />

              <FormControlLabel
                control={
                  <Checkbox
                    checked={showWarnings}
                    onChange={e => {
                      setShowWarnings(!showWarnings);
                      setPage(0);
                    }}
                    value="showWarnigs"
                  />
                }
                label="Show warning logs"
              />

              <FormControlLabel
                control={
                  <Checkbox
                    checked={showTrace}
                    onChange={e => {
                      setShowTrace(!showTrace);
                      setPage(0);
                    }}
                    value="showTrace"
                  />
                }
                label="Show trace logs"
              />
            </Grid>

            <TablePagination
              rowsPerPageOptions={[50, 100, 250, 500]}
              component="div"
              count={filteredLogs().length}
              rowsPerPage={rowsPerPage}
              page={page}
              backIconButtonProps={{
                "aria-label": "previous page"
              }}
              nextIconButtonProps={{
                "aria-label": "next page"
              }}
              onChangePage={handleChangePage}
              onChangeRowsPerPage={handleChangeRowsPerPage}
            />

            <Table
              className={classes.table}
              size="small"
              aria-label="a dense table"
            >
              <TableHead>
                <TableRow>
                  <TableCell component="th">Type</TableCell>
                  <TableCell component="th">Action</TableCell>
                  <TableCell component="th">Message</TableCell>
                  <TableCell component="th">Result</TableCell>
                  <TableCell component="th"></TableCell>

                  <TableCell component="th">Date</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                <LogEntry {...currentLog} />
              </TableBody>
            </Table>
            <TablePagination
              rowsPerPageOptions={[50, 100, 250]}
              component="div"
              count={filteredLogs().length}
              rowsPerPage={rowsPerPage}
              page={page}
              backIconButtonProps={{
                "aria-label": "previous page"
              }}
              nextIconButtonProps={{
                "aria-label": "next page"
              }}
              onChangePage={handleChangePage}
              onChangeRowsPerPage={handleChangeRowsPerPage}
            />
          </Grid>
        </Paper>

        <Modal
          aria-labelledby="simple-modal-title"
          aria-describedby="simple-modal-description"
          open={showTraceWindow}
          onClose={e => setShowTraceWindow(false)}
        >
          <Paper className={classes.log}>
            <pre dangerouslySetInnerHTML={{ __html: exceptionMsg }} />
          </Paper>
        </Modal>
      </Grid>
    );
  }

  return <Loader />;
}

export default Log;

// <TableRow key={`row-${index}`}>
// <TableCell>{o.connectionRuleName}</TableCell>
// <TableCell>{o.logAction}</TableCell>
// <TableCell>{o.logType}</TableCell>
// <TableCell>{o.logResult}</TableCell>
// <TableCell>{o.message}</TableCell>
// <TableCell>{o.dateCreated}</TableCell>
// </TableRow>
