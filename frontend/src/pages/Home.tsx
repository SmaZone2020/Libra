import { Button, Card } from '@heroui/react';

function Home() {
  return (
    <div className="container mx-auto py-8">
      <Card className="p-6">
        <h1 className="text-2xl font-bold mb-4">Welcome to HeroUI3</h1>
        <p className="mb-6">This is a basic example of HeroUI3 with Vite and React Router.</p>
        <Button>Get Started</Button>
      </Card>
    </div>
  );
}

export default Home;
