import Hero from "./components/Hero/Hero";
import Feature from "./components/Feature/Feature";

import "./App.css";

function App() {
  return (
    <div className="App">
      <Hero
        title={`Raise Capital\nwith Confidence`}
        description={`Connect with top-tier venture capital firms\nand accelerate your startup's growth.\nOur platform streamlines the fundraising process.`}
        h2Title="Success Stories"
        smallTitle={`Join thousands of founders\nwho've raised over $10B`}
        ctaText="Sign up"
        onCtaClick={() => {}}
      />
      <Feature />
    </div>
  );
}

export default App;
