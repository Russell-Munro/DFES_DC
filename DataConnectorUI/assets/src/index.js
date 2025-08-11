import React from 'react';
import ReactDOM from 'react-dom';
import 'typeface-roboto';
import App from './apps/App/App';
import { ThemeProvider } from '@material-ui/styles';
import { createMuiTheme } from "@material-ui/core";



const theme = createMuiTheme({
    spacing: 4,
    palette: {
      primary: {
        main: "#16c18f",
      },
    }
  });

const domContainer = document.querySelector('#connectionApp');
if(domContainer){
  ReactDOM.render(<ThemeProvider theme={theme}><App /></ThemeProvider>, domContainer); 

}
