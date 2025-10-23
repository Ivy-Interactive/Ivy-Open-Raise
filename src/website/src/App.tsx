import Hero from "./components/Hero/Hero";
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
        smallTitle="Join thousands of founders who've raised over $10B"
        ctaText="Explore Investors"
        onCtaClick={handleCtaClick}
      />
    </div>
  );
}

export default App;
