import Hero from "./components/Hero/Hero";
import Feature from "./components/Feature/Feature";
import "./App.css";

function App() {
  const handleCtaClick = () => {
    // TODO: Implement search or navigation to fund discovery
    console.log("Explore funds clicked");
  };

  return (
    <div className="App">
      <Hero
        title="Raise Capital with Confidence"
        description="Connect with top-tier venture capital firms and accelerate your startup's growth. Our platform streamlines the fundraising process from pitch to close."
        h2Title="Success Stories"
        smallTitle="Join thousands of founders who've raised over $10B"
        ctaText="Sign up"
        onCtaClick={handleCtaClick}
      />
      <Feature />
    </div>
  );
}

export default App;
