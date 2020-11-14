import React from 'react';
import './App.css';
import { BrowserRouter as Router, Route, Switch   } from "react-router-dom"
import { SamplePage } from './SamplePage';


function App() {
  return (
    <Router>
      <>
        <Switch>
          <Route path="/" component={SamplePage} />
        </Switch>
      </>
    </Router>
  )
}

export default App;
