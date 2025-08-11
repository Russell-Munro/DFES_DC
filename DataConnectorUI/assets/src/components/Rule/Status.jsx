import React from "react";
import { Typography } from "@material-ui/core";
import { mergeClasses, makeStyles } from "@material-ui/styles";

const useStyles = makeStyles(theme => ({
    progressWrapper:{
        display:"block",width:"100%",height:"4px",
        background:"#e0e0e0",
        padding:1,
    },
    progress:{
        backgroundColor: theme.palette.primary.main,
        height:"4px"}
    
}));

const Status = (props)=>{
    const classes = useStyles();

    return (
    <div  className={classes.progressWrapper}>
        <div>
            <div className={classes.progress} id="progress" style={{width:"0%"}}></div>
        </div>
    <Typography variant="caption" id="statusmessage"></Typography>
    </div>
    )

}

export default React.memo(Status)