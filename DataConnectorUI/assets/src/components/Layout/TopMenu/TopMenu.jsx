import React, { PureComponent, useState } from "react";
import { makeStyles } from "@material-ui/core/styles";
import AppBar from "@material-ui/core/AppBar";
import Toolbar from "@material-ui/core/Toolbar";
import Typography from "@material-ui/core/Typography";
import IconButton from "@material-ui/core/IconButton";
import MenuIcon from "@material-ui/icons/Menu";
import AccountCircle from "@material-ui/icons/AccountCircle";
import Switch from "@material-ui/core/Switch";
import FormControlLabel from "@material-ui/core/FormControlLabel";
import FormGroup from "@material-ui/core/FormGroup";
import MenuItem from "@material-ui/core/MenuItem";
import Menu from "@material-ui/core/Menu";
import { withRouter } from "react-router-dom";
import { Container, Grid, Button } from "@material-ui/core";
import { userInfo } from "os";
import PowerIcon from '@material-ui/icons/Power';
import PowerOffIcon from '@material-ui/icons/PowerOff';

const useStyles = makeStyles(theme => ({
  toolbar: {
    color: theme.palette.common.white
  }, 
  tools:{
    display: 'flex',
    alignContent:"center",
    justifyContent:"center",
    alignItems:"center"

  },
  grow: {
    flexGrow: 1,
  },
}));

function TopMenu(props) {
  const classes = useStyles();

  const [profileAnchorEl, setProfileAnchorEl] = useState(null);
  const [menuAnchorEl, setMenuAnchorEl] = useState(null);

  /**
   * MENU FUNCTIONS
   */

  const handleMenuClick = event => {
    console.log(event.currentTarget);
    setMenuAnchorEl(event.currentTarget);
  };

  const handleMenuSelect = path => {
    console.log(path);
    props.history.push(path);
    setMenuAnchorEl(null);
  };

  const handleMenuClose = () => {
    setMenuAnchorEl(null);
  };

  return (
    <div>
      <AppBar position="fixed">
      <Container maxWidth="lg">

        <Toolbar className={classes.toolbar}>
          <div>
            <IconButton
              edge="start"
              color="inherit"
              aria-label="menu"
              onClick={handleMenuClick}
            >
              <MenuIcon />
            </IconButton>
            <Menu
              id="simple-menu"
              anchorEl={menuAnchorEl}
              keepMounted
              open={Boolean(menuAnchorEl)}
              onClose={handleMenuClose}
            >
              <MenuItem onClick={() => handleMenuSelect("/connections/")}>
                Connections
              </MenuItem>
              <MenuItem onClick={() => handleMenuSelect("/logs/")}>
                Logs
              </MenuItem>
            </Menu>
          </div>
          <Typography variant="h6">DFES Sharepoint Connector</Typography>

          <div className={classes.grow} />
          <div className={classes.tools}>

          {props.webSocketState === WebSocket.OPEN &&
          <PowerIcon/>
          }

{props.webSocketState !== WebSocket.OPEN &&
          <PowerOffIcon />
          }
          {/* <Button onClick={(e) => props.websocket.close()}>kill</Button> */}

          <AccountCircle />

          <Typography variant="caption">&nbsp;{window.userInfo.Username}</Typography>
          </div>
        </Toolbar>
        </Container>
      </AppBar>
    </div>
  );
}

export default withRouter(TopMenu);
