import React from "react";
import { makeStyles } from "@material-ui/core";


const useStyles = makeStyles(theme => ({
    panel: {
      height:400,
      backgroundColor:"black",
      color:"white",
      overflow:"scroll"
    }
}));

const Transcript = (props) =>{

    const classes = useStyles();

    const TranscriptText = () => {
        return props.transcript.map((t,idx) => {
            //console.log(JSON.stringify(t))
            return(`${t}\n`)
          })
        };

    return (
        <div className={classes.panel}>
            <pre>
                <TranscriptText />
            </pre>

        </div>
    )
        

}

export default React.memo(Transcript);