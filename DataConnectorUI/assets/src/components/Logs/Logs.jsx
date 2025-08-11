import React, { useState, useEffect } from "react";
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
  TablePagination
} from "@material-ui/core";
import gql from "graphql-tag";
import SearchIcon from "@material-ui/icons/Search";
import { useQuery } from "@apollo/react-hooks";

import TimeAgo from "javascript-time-ago";
import en from "javascript-time-ago/locale/en";
import moment from "moment";
import FormattedDate from "../FormattedDate/FormattedDate";
import Loader from "../Loader/Loader";
TimeAgo.addLocale(en);
const timeAgo = new TimeAgo("en-US");

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
  actionBar: { marginTop: theme.spacing(4), marginBottom: theme.spacing(4) }
}));

const LOGS = gql`
  query getLogs($connectionRuleId: ID, $pageSize: Int, $pageNo: Int) {
    logs(
      connectionRuleId: $connectionRuleId
      pageSize: $pageSize
      pageNo: $pageNo
    ) {
      id
      connectionRuleName
      logAction
      logType
      logResult
      message
      dateCreated
      connectionRuleID
      stats
    }
    logCount(connectionRuleId: $connectionRuleId)
  }
`;

function Logs(props) {
  const classes = useStyles();

  const [rowsPerPage, setRowsPerPage] = useState(25);
  const [page, setPage] = useState(0);

  var { loading, error, data, fetchMore } = useQuery(LOGS, {
    variables: {
      connectionRuleId: props.match.params.connectionRuleId,
      pageSize: rowsPerPage,
      pageNo: page
    },
    notifyOnNetworkStatusChange: true
    //onCompleted:()=>setRowsPerPage(rowsPerPage)
  });


  const handleChangePage = (event, newPage) => {
    if ((newPage + 1) * rowsPerPage > data.logs.length) {
      fetchMore({
        variables: {
          pageSize: rowsPerPage,
          pageNo: newPage
        },
        updateQuery: (prev, { fetchMoreResult }) => {
          return Object.assign({}, prev, {
            logs: [...data.logs, ...fetchMoreResult.logs]
          });
        }
      });
    }

    setPage(newPage);
  };


  const handleChangeRowsPerPage = event => {
    setRowsPerPage(parseInt(event.target.value, 10));
    setPage(0);
  };

  return (
    <Grid container item xs={12}>
      <Grid item xs={12}>
        <Breadcrumbs aria-label="breadcrumb">
          <Link color="inherit" href="/#/">
            Home
          </Link>
          <Typography color="textPrimary">Logs</Typography>
        </Breadcrumbs>
      </Grid>

      <Grid item xs={12}>
        <Typography variant="h2" className={classes.pageTitle}>
          Logs
        </Typography>
      </Grid>

      <Paper className={classes.paper}>
        <Grid container item xs={12}>
          <Table
            className={classes.table}
            size="small"
            aria-label="a dense table"
          >
            <TableHead>
              <TableRow>
                <TableCell component="th">Rule Name</TableCell>

                <TableCell component="th">Action</TableCell>
                <TableCell component="th">Type</TableCell>
                <TableCell component="th">Result</TableCell>
                <TableCell component="th">Message</TableCell>

                <TableCell component="th">Date</TableCell>
                <TableCell component="th">View</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {data &&
                data.logs
                  .slice(page * rowsPerPage, page * rowsPerPage + rowsPerPage)
                  .map((o, index) => {
                    return (
                      <TableRow key={`row-${index}`}>
                        <TableCell>
                          {o.id} - {o.connectionRuleName}
                        </TableCell>
                        <TableCell>
                          {/* {console.log(o)} */}
                          <Link
                            color="inherit"
                            href={`/#/logs/${o.connectionRuleID}/${o.id}`}
                          >
                            {o.logAction}
                          </Link>
                        </TableCell>
                        <TableCell>
                          {" "}
                          <Link
                            color="inherit"
                            href={`/#/logs/${o.connectionRuleID}/${o.id}`}
                          >
                            {o.logType}
                          </Link>
                        </TableCell>
                        <TableCell>
                          {" "}
                          <Link
                            color="inherit"
                            href={`/#/logs/${o.connectionRuleID}/${o.id}`}
                          >
                            {o.stats.ExecutionStatus}
                          </Link>
                        </TableCell>
                        <TableCell>
                          {" "}
                          <Link
                            color="inherit"
                            href={`/#/logs/${o.connectionRuleID}/${o.id}`}
                          >
                            {o.message}
                          </Link>
                        </TableCell>
                        <TableCell>
                          {" "}
                          <Link
                            color="inherit"
                            href={`/#/logs/${o.connectionRuleID}/${o.id}`}
                          >
                            <FormattedDate date={o.dateCreated} />
                          </Link>
                        </TableCell>
                        <TableCell>
                          {" "}
                          <Link
                            color="inherit"
                            href={`/#/logs/${o.connectionRuleID}/${o.id}`}
                          >
                            <SearchIcon />
                          </Link>
                        </TableCell>
                      </TableRow>
                    );
                  })}
            </TableBody>
          </Table>

          {loading && <Loader />}

          {data && (
            <TablePagination
              rowsPerPageOptions={[10, 25, 50, 100]}
              component="div"
              count={data.logCount}
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
          )}
        </Grid>
      </Paper>
    </Grid>
  );
}

export default Logs;
